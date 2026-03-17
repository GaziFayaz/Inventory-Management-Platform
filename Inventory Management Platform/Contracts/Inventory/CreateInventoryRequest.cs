using Microsoft.AspNetCore.Http;

namespace Inventory_Management_Platform.Contracts.Inventory;

public sealed record CreateInventoryRequest(
    string Title,
    string? DescriptionMd,
    IFormFile? ImageFile,
    int? CategoryId,
    bool IsPublic,
    List<string>? TagNames
);