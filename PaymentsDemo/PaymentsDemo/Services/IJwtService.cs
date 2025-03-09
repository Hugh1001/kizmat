using System.Security.Claims;

namespace PaymentsDemo.Services;

public interface IJwtService
{
    string GenerateToken(Guid userId, string username);
    public void InvalidateToken(string tokenIdentifier, long expUnixTime);
    public bool IsTokenValid(ClaimsPrincipal user);
    public Guid? GetUserId(ClaimsPrincipal user);
}