namespace Inventory_Management_Platform.Contracts.Inventory;

public sealed record CreateInventoryRequest(
    string Title,
    string? DescriptionMd,
    string? ImageUrl,
    int? CategoryId,
    bool IsPublic,
    List<string>? TagNames
);