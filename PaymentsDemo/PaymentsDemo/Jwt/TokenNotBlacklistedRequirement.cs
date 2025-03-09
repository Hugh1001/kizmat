using Microsoft.AspNetCore.Authorization;
using PaymentsDemo.Services;

namespace PaymentsDemo.Jwt;

public class TokenNotBlacklistedHandler(IJwtService jwtService) : AuthorizationHandler<TokenNotBlacklistedRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, TokenNotBlacklistedRequirement requirement)
    {
        if (jwtService.IsTokenValid(context.User))
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail();
        }

        return Task.CompletedTask;
    }
}