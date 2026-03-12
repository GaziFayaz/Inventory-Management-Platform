using Inventory_Management_Platform.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace Inventory_Management_Platform.Common.Authorization;

/// <summary>
/// Satisfies <see cref="InventoryOwnerOrAdminRequirement"/> when the current user
/// is in the <c>Admin</c> role or is the owner of the target <see cref="Inventory"/>.
/// No DB query — all information comes from claims and the already-loaded resource.
/// </summary>
public class InventoryOwnerOrAdminHandler(UserManager<AppUser> userManager)
    : AuthorizationHandler<InventoryOwnerOrAdminRequirement, Inventory>
{
  protected override Task HandleRequirementAsync(
      AuthorizationHandlerContext context,
      InventoryOwnerOrAdminRequirement requirement,
      Inventory resource)
  {
    var userId = userManager.GetUserId(context.User);
    if (userId is null)
      return Task.CompletedTask;

    if (context.User.IsInRole("Admin") || userId == resource.OwnerId)
      context.Succeed(requirement);

    return Task.CompletedTask;
  }
}
