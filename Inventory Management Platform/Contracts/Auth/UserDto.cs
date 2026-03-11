namespace Inventory_Management_Platform.Contracts.Auth;

public sealed record UserDto(
    string   Id,
    string   DisplayName,
    string   Email,
    bool     IsAdmin,
    bool     IsBlocked,
    DateTime CreatedAt
);
