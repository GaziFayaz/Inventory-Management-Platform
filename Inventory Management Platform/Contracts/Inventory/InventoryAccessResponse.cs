namespace Inventory_Management_Platform.Contracts.Inventory;

public sealed record InventoryAccessResponse(
    Guid InventoryId,
    IReadOnlyList<string> Emails,
    uint Version
);