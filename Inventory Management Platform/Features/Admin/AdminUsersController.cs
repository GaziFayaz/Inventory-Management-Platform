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

    // ── PUT /admin/users/{id}/block ─────────────────────────────────────────
    [HttpPut("block/{id}")]
    public async Task<IActionResult> BlockUser(string id)
    {
        var dto = await adminUserService.BlockUserAsync(id, userManager.GetUserId(User)!);
        return Ok(ApiResponse.Ok(dto));
    }

    // ── PUT /admin/users/{id}/unblock ───────────────────────────────────────
    [HttpPut("unblock/{id}")]
    public async Task<IActionResult> UnblockUser(string id)
    {
        var dto = await adminUserService.UnblockUserAsync(id);
        return Ok(ApiResponse.Ok(dto));
    }

    // ── DELETE /admin/users/{id} ────────────────────────────────────────────
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        await adminUserService.DeleteUserAsync(id, userManager.GetUserId(User)!);
        return Ok(ApiResponse.Ok<object>(null!));
    }

    // ── POST /admin/users/{id}/roles/admin ──────────────────────────────────
    [HttpPost("roles/admin/{id}")]
    public async Task<IActionResult> PromoteToAdmin(string id)
    {
        var dto = await adminUserService.PromoteToAdminAsync(id);
        return Ok(ApiResponse.Ok(dto));
    }

    // ── DELETE /admin/users/{id}/roles/admin ────────────────────────────────
    // Self-demotion is explicitly allowed per spec.
    [HttpDelete("roles/admin/{id}")]
    public async Task<IActionResult> DemoteFromAdmin(string id)
    {
        var dto = await adminUserService.DemoteFromAdminAsync(id);
        return Ok(ApiResponse.Ok(dto));
    }

    // ── Helpers ─────────────────────────────────────────────────────────────
}
