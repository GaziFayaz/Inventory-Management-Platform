namespace Inventory_Management_Platform.Contracts.Inventory;

public sealed record InventoryDto(
    Guid Id,
    string OwnerId,
    string OwnerDisplayName,
    string Title,
    string? DescriptionMd,
    string? ImageUrl,
    int? CategoryId,
    string? CategoryName,
    bool IsPublic,
    IReadOnlyList<string> Tags,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    uint Version
);