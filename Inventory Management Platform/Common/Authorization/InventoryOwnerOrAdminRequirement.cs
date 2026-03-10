using Microsoft.AspNetCore.Authorization;

namespace Inventory_Management_Platform.Common.Authorization;

/// <summary>
/// Requires the user to be an admin (role check) or the owner of the target
/// <c>Inventory</c> resource. Used for settings, fields, custom-ID, and
/// access-list endpoints.
/// </summary>
public class InventoryOwnerOrAdminRequirement : IAuthorizationRequirement;
