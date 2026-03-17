namespace Inventory_Management_Platform.Contracts.Inventory;

public sealed record UpdateInventoryCustomFieldsRequest(
    List<CustomFieldDto> Fields,
    uint Version
);