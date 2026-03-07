namespace Inventory_Management_Platform.Models;

public class Item
{
  public Guid Id { get; set; }
  public Guid InventoryId { get; set; }
  public string? CreatedById { get; set; }

  /// <summary>
  /// The generated or user-edited custom ID string.
  /// Unique per inventory, enforced by composite DB index (InventoryId, CustomId).
  /// </summary>
  public string CustomId { get; set; } = string.Empty;

  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
  public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

  // --- String field values (slots 1–3) ---
  public string? String1Value { get; set; }
  public string? String2Value { get; set; }
  public string? String3Value { get; set; }

  // --- Multi-line text field values (slots 1–3) ---
  public string? MultiLine1Value { get; set; }
  public string? MultiLine2Value { get; set; }
  public string? MultiLine3Value { get; set; }

  // --- Numeric field values (slots 1–3) ---
  public decimal? Numeric1Value { get; set; }
  public decimal? Numeric2Value { get; set; }
  public decimal? Numeric3Value { get; set; }

  // --- Link/document field values (slots 1–3) ---
  public string? Link1Value { get; set; }
  public string? Link2Value { get; set; }
  public string? Link3Value { get; set; }

  // --- Boolean field values (slots 1–3) ---
  public bool? Bool1Value { get; set; }
  public bool? Bool2Value { get; set; }
  public bool? Bool3Value { get; set; }

  // Navigation
  public Inventory Inventory { get; set; } = null!;
  public AppUser? CreatedBy { get; set; }
  public ICollection<Like> Likes { get; set; } = [];
}
