using Inventory_Management_Platform.Contracts.Admin;
using Inventory_Management_Platform.Contracts.Auth;

namespace Inventory_Management_Platform.Features.Admin;

public interface IAdminUserService
{
    Task<UserListResponse> ListUsersAsync(int page, int pageSize, string? search);

    /// <param name="currentUserId">The acting admin's ID — prevents self-block.</param>
    Task<List<UserDto>> BlockUsersAsync(List<string> ids, string currentUserId);

    Task<List<UserDto>> UnblockUsersAsync(List<string> ids);

    /// <param name="currentUserId">The acting admin's ID — prevents self-delete.</param>
    Task DeleteUsersAsync(List<string> ids, string currentUserId);

    Task<List<UserDto>> PromoteToAdminsAsync(List<string> ids);

    /// <summary>Self-demotion is explicitly allowed.</summary>
    Task<List<UserDto>> DemoteFromAdminsAsync(List<string> ids);
}
