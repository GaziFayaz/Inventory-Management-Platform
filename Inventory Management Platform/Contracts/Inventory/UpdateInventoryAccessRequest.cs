namespace Inventory_Management_Platform.Contracts.Inventory;

public sealed record UpdateInventoryAccessRequest(
    List<string> Emails,
    uint Version
);