using Inventory_Management_Platform.Common.Errors;
using Inventory_Management_Platform.Contracts.Admin;
using Inventory_Management_Platform.Contracts.Auth;
using Inventory_Management_Platform.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Inventory_Management_Platform.Features.Admin;

public sealed class AdminUserService(UserManager<AppUser> userManager) : IAdminUserService
{
    private const int MaxPageSize = 100;

    public async Task<UserListResponse> ListUsersAsync(int page, int pageSize, string? search)
    {
        page     = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, MaxPageSize);

        var query = userManager.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(u =>
                u.DisplayName.ToLower().Contains(term) ||
                (u.Email != null && u.Email.ToLower().Contains(term)));
        }

        var totalCount = await query.CountAsync();

        var users = await query
            .OrderBy(u => u.DisplayName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new { u.Id, u.DisplayName, u.Email, u.IsBlocked, u.CreatedAt })
            .ToListAsync();

        // Batch-load admin role membership to avoid per-user DB queries.
        var adminIds = (await userManager.GetUsersInRoleAsync("Admin"))
            .Select(u => u.Id)
            .ToHashSet();

        var items = users.Select(u => new UserDto(
            u.Id,
            u.DisplayName,
            u.Email!,
            adminIds.Contains(u.Id),
            u.IsBlocked,
            u.CreatedAt
        )).ToList();

        return new UserListResponse(items, page, pageSize, totalCount);
    }

    public async Task<UserDto> BlockUserAsync(string id, string currentUserId)
    {
        if (id == currentUserId)
            throw new AppException(400, "You cannot block your own account.", ErrorCodes.Forbidden);

        var user = await LoadUserOrThrowAsync(id);
        user.IsBlocked = true;
        await userManager.UpdateAsync(user);

        return await BuildUserDtoAsync(user);
    }

    public async Task<UserDto> UnblockUserAsync(string id)
    {
        var user = await LoadUserOrThrowAsync(id);
        user.IsBlocked = false;
        await userManager.UpdateAsync(user);

        return await BuildUserDtoAsync(user);
    }

    public async Task DeleteUserAsync(string id, string currentUserId)
    {
        if (id == currentUserId)
            throw new AppException(400, "You cannot delete your own account.", ErrorCodes.CannotDeleteSelf);

        var user = await LoadUserOrThrowAsync(id);
        await userManager.DeleteAsync(user);
    }

    public async Task<UserDto> PromoteToAdminAsync(string id)
    {
        var user = await LoadUserOrThrowAsync(id);

        if (!await userManager.IsInRoleAsync(user, "Admin"))
            await userManager.AddToRoleAsync(user, "Admin");

        return await BuildUserDtoAsync(user);
    }

    public async Task<UserDto> DemoteFromAdminAsync(string id)
    {
        var user = await LoadUserOrThrowAsync(id);

        if (await userManager.IsInRoleAsync(user, "Admin"))
            await userManager.RemoveFromRoleAsync(user, "Admin");

        return await BuildUserDtoAsync(user);
    }

    // ── Private helpers ─────────────────────────────────────────────────────

    private async Task<AppUser> LoadUserOrThrowAsync(string id)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user is null)
            throw new AppException(404, "User not found.", ErrorCodes.UserNotFound);
        return user;
    }

    private async Task<UserDto> BuildUserDtoAsync(AppUser user)
    {
        var isAdmin = await userManager.IsInRoleAsync(user, "Admin");
        return new UserDto(
            user.Id,
            user.DisplayName,
            user.Email!,
            isAdmin,
            user.IsBlocked,
            user.CreatedAt
        );
    }
}
