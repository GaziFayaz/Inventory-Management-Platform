namespace Inventory_Management_Platform.Models;

/// <summary>A discussion post on an inventory. Append-only; no editing or deletion.</summary>
public class Post
{
  public Guid Id { get; set; }
  public Guid InventoryId { get; set; }
  public string AuthorId { get; set; } = string.Empty;
  public string ContentMd { get; set; } = string.Empty;
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

  // Navigation
  public Inventory Inventory { get; set; } = null!;
  public AppUser Author { get; set; } = null!;
}
