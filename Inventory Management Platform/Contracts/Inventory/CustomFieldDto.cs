namespace Inventory_Management_Platform.Contracts.Inventory;

public sealed record CustomFieldDto(
    CustomFieldType Type,
    int Slot,
    bool Enabled,
    string? Title,
    string? Description,
    bool ShowInTable,
    int OrderIndex
);