using Inventory_Management_Platform.Models.CustomId;
using NpgsqlTypes;

namespace Inventory_Management_Platform.Models;

public class Inventory
{
    public Guid Id { get; set; }
    public string OwnerId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? DescriptionMd { get; set; }
    public string? ImageUrl { get; set; }
    public int? CategoryId { get; set; }
    public bool IsPublic { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Stored as JSONB. Ordered list of CustomIdElement descriptors
    /// defining how item custom IDs are generated for this inventory.
    /// </summary>
    public List<CustomIdElement>? CustomIdFormat { get; set; }

    /// <summary>
    /// Computed tsvector column for full-text search over Title and DescriptionMd.
    /// Managed by Postgres; never set manually.
    /// </summary>
    public NpgsqlTsVector SearchVector { get; set; } = null!;

    // --- String fields (up to 3) ---
    public bool String1Enabled { get; set; }
    public string? String1Name { get; set; }
    public string? String1Description { get; set; }
    public bool String1ShowInTable { get; set; }
    public int String1OrderIndex { get; set; }

    public bool String2Enabled { get; set; }
    public string? String2Name { get; set; }
    public string? String2Description { get; set; }
    public bool String2ShowInTable { get; set; }
    public int String2OrderIndex { get; set; }

    public bool String3Enabled { get; set; }
    public string? String3Name { get; set; }
    public string? String3Description { get; set; }
    public bool String3ShowInTable { get; set; }
    public int String3OrderIndex { get; set; }

    // --- Multi-line text fields (up to 3) ---
    public bool MultiLine1Enabled { get; set; }
    public string? MultiLine1Name { get; set; }
    public string? MultiLine1Description { get; set; }
    public bool MultiLine1ShowInTable { get; set; }
    public int MultiLine1OrderIndex { get; set; }

    public bool MultiLine2Enabled { get; set; }
    public string? MultiLine2Name { get; set; }
    public string? MultiLine2Description { get; set; }
    public bool MultiLine2ShowInTable { get; set; }
    public int MultiLine2OrderIndex { get; set; }

    public bool MultiLine3Enabled { get; set; }
    public string? MultiLine3Name { get; set; }
    public string? MultiLine3Description { get; set; }
    public bool MultiLine3ShowInTable { get; set; }
    public int MultiLine3OrderIndex { get; set; }

    // --- Numeric fields (up to 3) ---
    public bool Numeric1Enabled { get; set; }
    public string? Numeric1Name { get; set; }
    public string? Numeric1Description { get; set; }
    public bool Numeric1ShowInTable { get; set; }
    public int Numeric1OrderIndex { get; set; }

    public bool Numeric2Enabled { get; set; }
    public string? Numeric2Name { get; set; }
    public string? Numeric2Description { get; set; }
    public bool Numeric2ShowInTable { get; set; }
    public int Numeric2OrderIndex { get; set; }

    public bool Numeric3Enabled { get; set; }
    public string? Numeric3Name { get; set; }
    public string? Numeric3Description { get; set; }
    public bool Numeric3ShowInTable { get; set; }
    public int Numeric3OrderIndex { get; set; }

    // --- Link/document fields (up to 3) ---
    public bool Link1Enabled { get; set; }
    public string? Link1Name { get; set; }
    public string? Link1Description { get; set; }
    public bool Link1ShowInTable { get; set; }
    public int Link1OrderIndex { get; set; }

    public bool Link2Enabled { get; set; }
    public string? Link2Name { get; set; }
    public string? Link2Description { get; set; }
    public bool Link2ShowInTable { get; set; }
    public int Link2OrderIndex { get; set; }

    public bool Link3Enabled { get; set; }
    public string? Link3Name { get; set; }
    public string? Link3Description { get; set; }
    public bool Link3ShowInTable { get; set; }
    public int Link3OrderIndex { get; set; }

    // --- Boolean fields (up to 3) ---
    public bool Bool1Enabled { get; set; }
    public string? Bool1Name { get; set; }
    public string? Bool1Description { get; set; }
    public bool Bool1ShowInTable { get; set; }
    public int Bool1OrderIndex { get; set; }

    public bool Bool2Enabled { get; set; }
    public string? Bool2Name { get; set; }
    public string? Bool2Description { get; set; }
    public bool Bool2ShowInTable { get; set; }
    public int Bool2OrderIndex { get; set; }

    public bool Bool3Enabled { get; set; }
    public string? Bool3Name { get; set; }
    public string? Bool3Description { get; set; }
    public bool Bool3ShowInTable { get; set; }
    public int Bool3OrderIndex { get; set; }

    // Navigation
    public AppUser Owner { get; set; } = null!;
    public Category? Category { get; set; }
    public ICollection<Item> Items { get; set; } = [];
    public ICollection<Tag> Tags { get; set; } = [];
    public ICollection<InventoryAccess> AccessList { get; set; } = [];
    public ICollection<Post> Posts { get; set; } = [];
}
