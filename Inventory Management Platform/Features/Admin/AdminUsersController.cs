using Inventory_Management_Platform.Common;
using Inventory_Management_Platform.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Inventory_Management_Platform.Features.Admin;

[ApiController]
[Route("admin/users")]
[Authorize(Policy = "Admin")]
public sealed class AdminUsersController(
    UserManager<AppUser> userManager,
    IAdminUserService    adminUserService) : ControllerBase
{
    // ── GET /admin/users?page=1&pageSize=20&search= ─────────────────────────
    [HttpGet]
    public async Task<IActionResult> ListUsers(
        [FromQuery] int     page     = 1,
        [FromQuery] int     pageSize = 20,
        [FromQuery] string? search   = null)
    {
        var result = await adminUserService.ListUsersAsync(page, pageSize, search);
        return Ok(ApiResponse.Ok(result));
    }

    // ── PUT /admin/users/block ──────────────────────────────────────────────
    [HttpPut("block")]
    public async Task<IActionResult> BlockUsers([FromBody] List<string> userIds)
    {
        var dtos = await adminUserService.BlockUsersAsync(userIds, userManager.GetUserId(User)!);
        return Ok(ApiResponse.Ok(dtos));
    }

    // ── PUT /admin/users/unblock ────────────────────────────────────────────
    [HttpPut("unblock")]
    public async Task<IActionResult> UnblockUsers([FromBody] List<string> userIds)
    {
        var dtos = await adminUserService.UnblockUsersAsync(userIds);
        return Ok(ApiResponse.Ok(dtos));
    }

    // ── DELETE /admin/users ─────────────────────────────────────────────────
    [HttpDelete]
    public async Task<IActionResult> DeleteUsers([FromBody] List<string> userIds)
    {
        await adminUserService.DeleteUsersAsync(userIds, userManager.GetUserId(User)!);
        return Ok(ApiResponse.Ok<object>(null!));
    }

    // ── POST /admin/users/roles/admin ───────────────────────────────────────
    [HttpPost("roles/admin")]
    public async Task<IActionResult> PromoteToAdmins([FromBody] List<string> userIds)
    {
        var dtos = await adminUserService.PromoteToAdminsAsync(userIds);
        return Ok(ApiResponse.Ok(dtos));
    }

    // ── DELETE /admin/users/roles/admin ─────────────────────────────────────
    // Self-demotion is explicitly allowed per spec.
    [HttpDelete("roles/admin")]
    public async Task<IActionResult> DemoteFromAdmins([FromBody] List<string> userIds)
    {
        var dtos = await adminUserService.DemoteFromAdminsAsync(userIds);
        return Ok(ApiResponse.Ok(dtos));
    }
}
