namespace Inventory_Management_Platform.Models;

/// <summary>
/// Many-to-many join between Inventory and Tag.
/// </summary>
public class InventoryTag
{
  public Guid InventoryId { get; set; }
  public int TagId { get; set; }

  // Navigation
  public Inventory Inventory { get; set; } = null!;
  public Tag Tag { get; set; } = null!;
}
