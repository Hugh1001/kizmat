using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PaymentsDemo.Extensions;
using PaymentsDemo.Options;

namespace PaymentsDemo.Services;

public class JwtService(IMemoryCache cache, IOptions<JwtOptions> options) : IJwtService
{
    private readonly JwtOptions _jwtOptions = options.Value;

    public string GenerateToken(Guid userId, string username)
    {
        var expiration = DateTime.UtcNow.AddMinutes(_jwtOptions.TokenLifetimeMinutes);

        var token = new JwtSecurityToken(
            claims:
            [
                new Claim(JwtRegisteredClaimNames.Sub, username),
                new Claim("UserId", userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            ],
            expires: expiration,
            signingCredentials: new SigningCredentials(_jwtOptions.Secret.ToSecurityKey(), SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Save token to cache until it expired to invalidate in further requests
    /// </summary>
    public void InvalidateToken(string tokenIdentifier, long expUnixTime)
    {
        var absoluteExpirationRelativeToNow = DateTimeOffset.FromUnixTimeSeconds(expUnixTime) - DateTimeOffset.UtcNow;

        // Do not invalidate already expired tokens
        if (absoluteExpirationRelativeToNow > TimeSpan.FromSeconds(0))
        {
            cache.Set(tokenIdentifier, true, absoluteExpirationRelativeToNow);   
        }
    }

    /// <summary>
    /// Checks whether token identifier is blacklisted
    /// </summary>
    public bool IsTokenValid(ClaimsPrincipal user)
    {
        var jti = user.FindFirstValue(JwtRegisteredClaimNames.Jti);

        return !string.IsNullOrEmpty(jti) && cache.Get(jti) == null;
    }

    public Guid? GetUserId(ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirstValue("UserId");

        if (Guid.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }

        return null;
    }
}