using Microsoft.AspNetCore.Authorization;

namespace PaymentsDemo.Jwt;

public class TokenNotBlacklistedRequirement : IAuthorizationRequirement
{
    public const string AuthorizationRequirementName = "BlacklistedTokenCheck";
}