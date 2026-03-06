namespace Inventory_Management_Platform.Models.CustomId;

/// <summary>
/// Represents one user-configured element in an inventory's custom ID format.
/// Stored as part of a JSONB array in Inventory.CustomIdFormat.
/// </summary>
public sealed class CustomIdElement
{
    /// <summary>The type of this element, serialized as a string into JSONB.</summary>
    public CustomIdElementType Type { get; set; }

    /// <summary>
    /// The format pattern the user entered (e.g. "D4", "yyyyMMdd", "INV-").
    /// Null for types where HasPattern = false (Guid, Random6Digit, Random9Digit).
    /// </summary>
    public string? Pattern { get; set; }
}
