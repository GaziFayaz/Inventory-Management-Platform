using Inventory_Management_Platform.Contracts.Auth;

namespace Inventory_Management_Platform.Contracts.Admin;

public sealed record UserListResponse(
    IReadOnlyList<UserDto> Items,
    int                    Page,
    int                    PageSize,
    int                    TotalCount
);
