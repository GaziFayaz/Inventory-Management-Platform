using Inventory_Management_Platform.Data;
using Inventory_Management_Platform.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Inventory_Management_Platform.Common.Authorization;

/// <summary>
/// Satisfies <see cref="InventoryWriteRequirement"/> when the current user has write
/// access to the target <see cref="Inventory"/> via any of these checks (in order):
/// <list type="number">
///   <item>User is in the <c>Admin</c> role.</item>
///   <item>User is the inventory owner.</item>
///   <item>User has an explicit <c>InventoryAccess</c> row — one DB query.</item>
///   <item>The inventory is public and the user is authenticated.</item>
/// </list>
/// </summary>
public class InventoryWriteHandler(
    UserManager<AppUser> userManager,
    AppDbContext db)
    : AuthorizationHandler<InventoryWriteRequirement, Inventory>
{
  protected override async Task HandleRequirementAsync(
      AuthorizationHandlerContext context,
      InventoryWriteRequirement requirement,
      Inventory resource)
  {
    var userId = userManager.GetUserId(context.User);
    if (userId is null)
      return;

    if (context.User.IsInRole("Admin") || userId == resource.OwnerId)
    {
      context.Succeed(requirement);
      return;
    }

    var hasAccess = await db.InventoryAccesses
        .AnyAsync(a => a.InventoryId == resource.Id && a.UserId == userId);

    if (hasAccess)
    {
      context.Succeed(requirement);
      return;
    }

    if (resource.IsPublic)
      context.Succeed(requirement);
  }
}
