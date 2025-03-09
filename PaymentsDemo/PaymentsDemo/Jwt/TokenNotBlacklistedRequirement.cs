using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;

namespace PaymentsDemo.Jwt;

public class TokenNotBlacklistedHandler(IMemoryCache cache)
    : AuthorizationHandler<TokenNotBlacklistedRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, TokenNotBlacklistedRequirement requirement)
    {
        var jti = context.User.FindFirstValue(JwtRegisteredClaimNames.Jti);
        if (string.IsNullOrEmpty(jti) || cache.Get(jti) != null)
        {
            context.Fail();
        }
        else
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}