using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using PaymentsDemo.Dto;
using PaymentsDemo.Extensions;
using PaymentsDemo.Jwt;
using PaymentsDemo.Options;
using PaymentsDemo.Services;

var builder = WebApplication.CreateBuilder(args);

var jwtConfigSection = builder.Configuration.GetSection(JwtOptions.SectionName);
builder.Services.Configure<JwtOptions>(jwtConfigSection);
builder.Services.Configure<DbOptions>(builder.Configuration.GetSection(DbOptions.SectionName));

builder.Services.AddMemoryCache();
builder.Services.AddSingleton<IJwtService, JwtService>();
builder.Services.AddSingleton<IUserService, UserService>();
builder.Services.AddSingleton<IAuthorizationHandler, TokenNotBlacklistedHandler>();
builder.Services.AddSingleton<IBalanceService, BalanceService>();

// Configure Rate Limiting
var rateLimitName = "fixed-ip-limit";
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy(rateLimitName, httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 2,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
});

// Configure JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtSecret = builder.Configuration["Jwt:Secret"]
                        ?? throw new InvalidOperationException("JWT secret key is missing.");

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = jwtSecret.ToSecurityKey()
        };
    });

builder.Services.AddAuthorization(options =>
    options.AddPolicy(
        TokenNotBlacklistedRequirement.AuthorizationRequirementName,
        policy => policy.Requirements.Add(new TokenNotBlacklistedRequirement())));

var app = builder.Build();

app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

// Register account endpoint
app.MapPost("/register", async (UserDto register, IUserService userService) =>
{
    var userExists = await userService.GetUserByUsernameAsync(register.Username);
    if (userExists is not null)
    {
        return Results.Conflict("Username already exists");   
    }

    await userService.CreateUserAsync(register.Username, register.Password);
    return Results.Ok("User registered successfully");
});

// Login endpoint, returns jwt token
app.MapPost("/login", async (UserDto login, IJwtService jwtService, IUserService userService) =>
{
    var user = await userService.GetUserByUsernameAsync(login.Username);
    if (user is null || !userService.VerifyPassword(login.Password, user.PasswordHash))
    {
        return Results.Unauthorized();
    }

    return Results.Ok(jwtService.GenerateToken(user.Id, user.Username));
}).RequireRateLimiting(rateLimitName);

// log out endpoint, mark token as invalid using in memory cache
app.MapPost("/logout", (ClaimsPrincipal user, IMemoryCache cache) =>
{
    var jti = user.FindFirstValue(JwtRegisteredClaimNames.Jti);
    var expClaim = user.FindFirstValue(JwtRegisteredClaimNames.Exp);

    if (jti is null || !long.TryParse(expClaim, out var expUnixTime))
    {
        return Task.FromResult(Results.BadRequest("Invalid token"));   
    }

    var expiryTime = DateTimeOffset.FromUnixTimeSeconds(expUnixTime);
    cache.Set(jti, true, expiryTime - DateTimeOffset.UtcNow);

    return Task.FromResult(Results.Ok("Logged out successfully"));
}).RequireAuthorization(TokenNotBlacklistedRequirement.AuthorizationRequirementName);

// execute payment endpoint, check user account existence, create if not exists, and update balance using positive concurrency
app.MapGet("/execute-payment", async (ClaimsPrincipal user, [FromServices] IBalanceService balanceService) =>
{
    var userIdClaim = user.FindFirstValue("UserId");
    if (!Guid.TryParse(userIdClaim, out var userId))
    {
        return Results.Unauthorized();   
    }

    var userBalance = await balanceService.GetBalanceAsync(userId);
    if (userBalance is null)
    {
        await balanceService.CreateUserBalanceAsync(userId, 8);
    }

    var updateAmount = 1m;
    var success = await balanceService.UpdateBalanceAsync(userId, -updateAmount);

    return success
        ? Results.Ok($"Balance decreased successfully by {updateAmount}.")
        : Results.BadRequest("Insufficient balance or concurrent update conflict.");
}).RequireAuthorization(TokenNotBlacklistedRequirement.AuthorizationRequirementName);

app.Run();
