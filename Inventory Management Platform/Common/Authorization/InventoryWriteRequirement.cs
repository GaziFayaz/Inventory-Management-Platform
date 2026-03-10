using Microsoft.AspNetCore.Authorization;

namespace Inventory_Management_Platform.Common.Authorization;

/// <summary>
/// Requires the user to have write access to the target <c>Inventory</c> resource.
/// Satisfied by any of:
/// <list type="bullet">
///   <item>User is an admin.</item>
///   <item>User is the inventory owner.</item>
///   <item>User has an explicit <c>InventoryAccess</c> row for this inventory.</item>
///   <item>The inventory is public and the user is authenticated.</item>
/// </list>
/// Used for item CRUD and discussion-post endpoints.
/// </summary>
public class InventoryWriteRequirement : IAuthorizationRequirement;
