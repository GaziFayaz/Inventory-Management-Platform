using Inventory_Management_Platform.Contracts.Admin;
using Inventory_Management_Platform.Contracts.Auth;

namespace Inventory_Management_Platform.Features.Admin;

public interface IAdminUserService
{
    Task<UserListResponse> ListUsersAsync(int page, int pageSize, string? search);

    /// <param name="currentUserId">The acting admin's ID — prevents self-block.</param>
    Task<UserDto> BlockUserAsync(string id, string currentUserId);

    Task<UserDto> UnblockUserAsync(string id);

    /// <param name="currentUserId">The acting admin's ID — prevents self-delete.</param>
    Task DeleteUserAsync(string id, string currentUserId);

    Task<UserDto> PromoteToAdminAsync(string id);

    /// <summary>Self-demotion is explicitly allowed.</summary>
    Task<UserDto> DemoteFromAdminAsync(string id);
}
