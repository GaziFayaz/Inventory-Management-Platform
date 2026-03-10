using Microsoft.AspNetCore.Authorization;

namespace Inventory_Management_Platform.Common.Authorization;

/// <summary>
/// Requires that the authenticated user's <c>IsBlocked</c> flag is <c>false</c>.
/// Compose this requirement into every policy that targets authenticated users.
/// </summary>
public class NotBlockedRequirement : IAuthorizationRequirement;
