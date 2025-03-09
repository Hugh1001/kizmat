using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using PaymentsDemo.Dto;
using PaymentsDemo.Repositories;
using PaymentsDemo.Services;

namespace PaymentsDemo;

public static class Endpoints
{
    /// <summary>
    /// Register a new user, save user password hash to database
    /// </summary>
    public static async Task<IResult> Register(ILoginService loginService, UserDto newUser)
    {
        var result = await loginService.RegisterUser(newUser.Username, newUser.Password);
        if (result.IsFailed)
        {
            return Results.Conflict("Username already exists");   
        }

        return Results.Ok("User registered successfully");
    }
    
    /// <summary>
    /// Login, validates login and password by password hash in database and returns jwt token
    /// </summary>
    public static async Task<IResult> Login(ILoginService loginService, IJwtService jwtService, IUserRepository userRepository, UserDto login)
    {
        var result = await loginService.Login(login.Username, login.Password);

        return result.IsFailed ? Results.Unauthorized() : Results.Ok(result.Value);
    }

    /// <summary>
    /// log out, mark token as invalid by its jti in inMemoryCache
    /// </summary>
    public static IResult LogOut(ClaimsPrincipal user, IJwtService jwtService)
    {
        var tokenIdentifier = user.FindFirstValue(JwtRegisteredClaimNames.Jti);
        var expClaim = user.FindFirstValue(JwtRegisteredClaimNames.Exp);

        if (tokenIdentifier is null || !long.TryParse(expClaim, out var expUnixTime))
        {
            return Results.BadRequest("Invalid token");   
        }

        jwtService.InvalidateToken(tokenIdentifier, expUnixTime);

        return Results.Ok("Logged out successfully");
    }
    
    /// <summary>
    /// Make payment by call
    /// </summary>
    public static async Task<IResult> ExecutePayment(ClaimsPrincipal user, IJwtService jwtService, IPaymentsService paymentsService)
    {
        var userId = jwtService.GetUserId(user);
        if (!userId.HasValue)
        {
            return Results.Unauthorized();   
        }

        await paymentsService.MakePaymentAsync(userId.Value);

        return Results.Ok($"Payment executed successfully.");
    }
}