using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using PaymentsDemo;
using PaymentsDemo.Extensions;
using PaymentsDemo.Jwt;
using PaymentsDemo.Options;
using PaymentsDemo.Repositories;
using PaymentsDemo.Services;

var builder = WebApplication.CreateBuilder(args);
var rateLimitName = "fixed-ip-limit";
ConfigureServices();

var app = builder.Build();

ConfigureApp();

ConfigureEndpoints();

app.Run();

void ConfigureServices()
{
    builder.Services.AddMemoryCache();
    builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
    builder.Services.Configure<DbOptions>(builder.Configuration.GetSection(DbOptions.SectionName));
    builder.Services.AddSingleton<IUserRepository, UserRepository>();
    builder.Services.AddSingleton<IUserBalanceRepository, UserBalanceRepository>();
    builder.Services.AddSingleton<ILoginService, LoginService>();
    builder.Services.AddSingleton<IJwtService, JwtService>();
    builder.Services.AddSingleton<IPaymentsService, PaymentsService>();
    builder.Services.AddSingleton<IAuthorizationHandler, TokenNotBlacklistedHandler>();

    // Configure Rate Limiting
    builder.Services.AddRateLimiter(options =>
    {
        options.AddPolicy(rateLimitName, httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 5,
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

    builder.Services.AddAuthorizationBuilder()
        .AddPolicy(TokenNotBlacklistedRequirement.AuthorizationRequirementName, policy => policy.Requirements.Add(new TokenNotBlacklistedRequirement()));
}

void ConfigureApp()
{
    app.UseRateLimiter();
    app.UseAuthentication();
    app.UseAuthorization();
}

void ConfigureEndpoints()
{
    app.MapPost("api/register", Endpoints.Register);

    app.MapPost("api/login", Endpoints.Login).RequireRateLimiting(rateLimitName);

    app.MapPost("api/logout", Endpoints.LogOut).RequireAuthorization(TokenNotBlacklistedRequirement.AuthorizationRequirementName);

    app.MapGet("api/execute-payment", Endpoints.ExecutePayment).RequireAuthorization(TokenNotBlacklistedRequirement.AuthorizationRequirementName);
}