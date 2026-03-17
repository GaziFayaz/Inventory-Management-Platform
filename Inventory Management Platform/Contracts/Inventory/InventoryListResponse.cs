namespace Inventory_Management_Platform.Contracts.Inventory;

public sealed record InventoryListResponse(
    IReadOnlyList<InventoryDto> Items,
    int Page,
    int PageSize,
    int TotalCount
);