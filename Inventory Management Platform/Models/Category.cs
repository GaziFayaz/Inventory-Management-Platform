namespace Inventory_Management_Platform.Models;

public class Category
{
  public int Id { get; set; }
  public string Name { get; set; } = string.Empty;

  // Navigation
  public ICollection<Inventory> Inventories { get; set; } = [];
}
