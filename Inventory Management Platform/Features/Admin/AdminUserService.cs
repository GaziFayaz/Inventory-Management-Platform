using Inventory_Management_Platform.Common.Errors;
using Inventory_Management_Platform.Contracts.Admin;
using Inventory_Management_Platform.Contracts.Auth;
using Inventory_Management_Platform.Models;
using Inventory_Management_Platform.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Inventory_Management_Platform.Features.Admin;

public sealed class AdminUserService(UserManager<AppUser> userManager, AppDbContext dbContext) : IAdminUserService
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

    public async Task<List<UserDto>> BlockUsersAsync(List<string> ids, string currentUserId)
    {
        if (ids.Contains(currentUserId))
            throw new AppException(400, "You cannot block your own account.", ErrorCodes.Forbidden);

        var users = await LoadUsersOrThrowAsync(ids);
        
        foreach (var user in users)
        {
            user.IsBlocked = true;
            await userManager.UpdateAsync(user);
            await userManager.UpdateSecurityStampAsync(user);
        }

        return await BuildUserDtosAsync(users);
    }

    public async Task<List<UserDto>> UnblockUsersAsync(List<string> ids)
    {
        var users = await LoadUsersOrThrowAsync(ids);

        foreach (var user in users)
        {
            user.IsBlocked = false;
            await userManager.UpdateAsync(user);
        }

        return await BuildUserDtosAsync(users);
    }

    public async Task DeleteUsersAsync(List<string> ids, string currentUserId)
    {
        if (ids.Contains(currentUserId))
            throw new AppException(400, "You cannot delete your own account.", ErrorCodes.CannotDeleteSelf);

        var users = await LoadUsersOrThrowAsync(ids);
        
        foreach (var user in users)
        {
            await userManager.DeleteAsync(user);
        }
    }

    public async Task<List<UserDto>> PromoteToAdminsAsync(List<string> ids)
    {
        var users = await LoadUsersOrThrowAsync(ids);

        foreach (var user in users)
        {
            await userManager.AddToRoleAsync(user, "Admin");
        }

        return await BuildUserDtosAsync(users);
    }

    public async Task<List<UserDto>> DemoteFromAdminsAsync(List<string> ids)
    {
        var users = await LoadUsersOrThrowAsync(ids);

        foreach (var user in users)
        {
            await userManager.RemoveFromRoleAsync(user, "Admin");
        }

        return await BuildUserDtosAsync(users);
    }

    private async Task<List<AppUser>> LoadUsersOrThrowAsync(List<string> ids)
    {
        if (ids is null || ids.Count == 0)
            return [];

        var distinctIds = ids.Distinct().ToList();
        var users = await userManager.Users
            .Where(u => distinctIds.Contains(u.Id))
            .ToListAsync();
            
        if (users.Count != distinctIds.Count)
            throw new AppException(404, "One or more users not found.", ErrorCodes.UserNotFound);
            
        return users;
    }

    private async Task<List<UserDto>> BuildUserDtosAsync(List<AppUser> users)
    {
        var adminIds = (await userManager.GetUsersInRoleAsync("Admin"))
            .Select(u => u.Id)
            .ToHashSet();

        return users.Select(user => new UserDto(
            user.Id,
            user.DisplayName,
            user.Email!,
            adminIds.Contains(user.Id),
            user.IsBlocked,
            user.CreatedAt
        )).ToList();
    }
}
