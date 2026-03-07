namespace Inventory_Management_Platform.Models;

/// <summary>
/// One like per user per item. Composite PK (ItemId, UserId) enforces uniqueness at DB level.
/// </summary>
public class Like
{
  public Guid ItemId { get; set; }
  public string UserId { get; set; } = string.Empty;

  // Navigation
  public Item Item { get; set; } = null!;
  public AppUser User { get; set; } = null!;
}
