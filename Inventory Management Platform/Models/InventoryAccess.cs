namespace Inventory_Management_Platform.Models;

/// <summary>
/// Grants write access to a specific user for a private inventory.
/// Membership in this table is the write-access signal — no role column needed
/// since there is only one non-owner permission tier per the spec.
/// </summary>
public class InventoryAccess
{
    public Guid InventoryId { get; set; }
    public string UserId { get; set; } = string.Empty;

    // Navigation
    public Inventory Inventory { get; set; } = null!;
    public AppUser User { get; set; } = null!;
}
