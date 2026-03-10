using Inventory_Management_Platform.Common.Errors;
using Inventory_Management_Platform.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace Inventory_Management_Platform.Common.Authorization;

/// <summary>
/// Satisfies <see cref="NotBlockedRequirement"/> when the current user's
/// <c>IsBlocked</c> flag is <c>false</c>. Issues one DB lookup per request.
/// </summary>
public class NotBlockedHandler(UserManager<AppUser> userManager)
    : AuthorizationHandler<NotBlockedRequirement>
{
  protected override async Task HandleRequirementAsync(
      AuthorizationHandlerContext context,
      NotBlockedRequirement requirement)
  {
    var userId = userManager.GetUserId(context.User);
    if (userId is null)
    {
      context.Fail();
      return;
    }

    var user = await userManager.FindByIdAsync(userId);
    if (user is null)
    {
      context.Fail();
      return;
    }

    if (user.IsBlocked)
    {
      context.Fail(new AuthorizationFailureReason(this, ErrorCodes.Blocked));
      return;
    }

    context.Succeed(requirement);
  }
}
