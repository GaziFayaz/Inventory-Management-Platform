using Inventory_Management_Platform.Common;
using Inventory_Management_Platform.Common.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;

namespace Inventory_Management_Platform.Common.Authorization;

/// <summary>
/// Replaces the default authorization middleware result handler so that
/// authorization failures return a consistent <see cref="ApiErrorResponse"/>
/// JSON body instead of an empty 401/403 response.
/// </summary>
public class AuthorizationResultHandler : IAuthorizationMiddlewareResultHandler
{
    private readonly AuthorizationMiddlewareResultHandler _default = new();

    public async Task HandleAsync(
        RequestDelegate next,
        HttpContext context,
        AuthorizationPolicy policy,
        PolicyAuthorizationResult authorizeResult)
    {
        if (authorizeResult.Succeeded)
        {
            await next(context);
            return;
        }

        // Not authenticated at all → 401
        if (authorizeResult.Challenged)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(
                ApiResponse.Fail(401, "Unauthorized.", ErrorCodes.Unauthorized));
            return;
        }

        // Authenticated but authorization failed → 403
        if (authorizeResult.Forbidden)
        {
            var isBlocked = authorizeResult.AuthorizationFailure?.FailureReasons
                .Any(r => r.Message == ErrorCodes.Blocked) ?? false;

            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(isBlocked
                ? ApiResponse.Fail(403, "Your account has been blocked.", ErrorCodes.Blocked)
                : ApiResponse.Fail(403, "Access denied.", ErrorCodes.Forbidden));
            return;
        }

        // Fallback to default behavior for any other case.
        await _default.HandleAsync(next, context, policy, authorizeResult);
    }
}
