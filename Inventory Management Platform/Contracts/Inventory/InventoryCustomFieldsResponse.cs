namespace Inventory_Management_Platform.Contracts.Inventory;

public sealed record InventoryCustomFieldsResponse(
    Guid InventoryId,
    IReadOnlyList<CustomFieldDto> Fields,
    uint Version
);