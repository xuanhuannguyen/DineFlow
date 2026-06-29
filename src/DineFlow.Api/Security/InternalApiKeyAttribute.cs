using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DineFlow.Api.Security;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class InternalApiKeyAttribute : Attribute, IAuthorizationFilter
{
    private const string HeaderName = "X-DineFlow-Internal-Key";

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var configuration = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
        var expectedKey = configuration["Security:InternalApiKey"];

        if (string.IsNullOrWhiteSpace(expectedKey))
        {
            context.Result = new ObjectResult(new { message = "Internal API key is not configured." })
            {
                StatusCode = StatusCodes.Status503ServiceUnavailable
            };
            return;
        }

        if (!context.HttpContext.Request.Headers.TryGetValue(HeaderName, out var providedKey)
            || !string.Equals(providedKey.ToString(), expectedKey, StringComparison.Ordinal))
        {
            context.Result = new UnauthorizedObjectResult(new { message = "Invalid internal API key." });
        }
    }
}
