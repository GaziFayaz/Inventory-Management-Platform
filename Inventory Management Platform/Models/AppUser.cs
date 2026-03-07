using Microsoft.AspNetCore.Identity;

namespace Inventory_Management_Platform.Models;

public class AppUser : IdentityUser
{
  public string DisplayName { get; set; } = string.Empty;
  public bool IsBlocked { get; set; } = false;
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

  // Navigation
  public ICollection<Inventory> OwnedInventories { get; set; } = [];
  public ICollection<InventoryAccess> InventoryAccesses { get; set; } = [];
  public ICollection<Item> CreatedItems { get; set; } = [];
  public ICollection<Post> Posts { get; set; } = [];
  public ICollection<Like> Likes { get; set; } = [];
}
