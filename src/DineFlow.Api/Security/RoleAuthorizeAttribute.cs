using DineFlow.BusinessObjects.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DineFlow.Api.Security;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class RoleAuthorizeAttribute : Attribute, IAuthorizationFilter
{
    private readonly HashSet<UserRole> _allowedRoles;

    public RoleAuthorizeAttribute(params UserRole[] allowedRoles)
    {
        _allowedRoles = allowedRoles.ToHashSet();
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var authorization = context.HttpContext.Request.Headers.Authorization.ToString();
        if (!authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            context.Result = new UnauthorizedObjectResult(new { message = "Bearer token is required." });
            return;
        }

        var tokenService = context.HttpContext.RequestServices.GetRequiredService<IApiTokenService>();
        var token = authorization["Bearer ".Length..].Trim();
        UserRole role;
        try
        {
            if (tokenService.TryReadRole(token, out role))
            {
                if (_allowedRoles.Contains(role))
                {
                    return;
                }

                context.Result = new ObjectResult(new { message = "User role is not allowed for this operation." })
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
                return;
            }
        }
        catch (InvalidOperationException ex)
        {
            context.Result = new ObjectResult(new { message = ex.Message })
            {
                StatusCode = StatusCodes.Status503ServiceUnavailable
            };
            return;
        }

        context.Result = new UnauthorizedObjectResult(new { message = "Invalid or expired token." });
    }
}
