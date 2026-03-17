using Inventory_Management_Platform.Common;
using Inventory_Management_Platform.Common.Errors;
using Inventory_Management_Platform.Contracts.Inventory;
using Inventory_Management_Platform.Data;
using Inventory_Management_Platform.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Inventory_Management_Platform.Features.Inventory;

[ApiController]
[Route("inventories")]
public sealed class InventoriesController(
    IInventoryService inventoryService,
    AppDbContext dbContext,
    IAuthorizationService authorizationService,
    UserManager<AppUser> userManager) : ControllerBase
{
    [HttpPost]
    [Authorize(Policy = "Authenticated")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> CreateInventory([FromForm] CreateInventoryRequest request)
    {
        var ownerId = userManager.GetUserId(User)!;
        var dto = await inventoryService.CreateAsync(ownerId, request);
        return StatusCode(StatusCodes.Status201Created, ApiResponse.Ok(dto, 201));
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> ListInventories(
        [FromQuery] string? ownerId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await inventoryService.ListAsync(ownerId, page, pageSize);
        return Ok(ApiResponse.Ok(result));
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetInventoryById(Guid id)
    {
        var dto = await inventoryService.GetByIdAsync(id);
        return Ok(ApiResponse.Ok(dto));
    }

    [HttpGet("{id}/access")]
    [Authorize(Policy = "Authenticated")]
    public async Task<IActionResult> GetAccess(Guid id)
    {
        var inventory = await dbContext.Inventories.FindAsync(id);
        if (inventory is null)
            return NotFound(ApiResponse.Fail(404, "Inventory was not found.", ErrorCodes.InventoryNotFound));

        var auth = await authorizationService.AuthorizeAsync(User, inventory, "OwnerOrAdmin");
        if (!auth.Succeeded)
            return Forbid();

        var response = await inventoryService.GetAccessAsync(id);
        return Ok(ApiResponse.Ok(response));
    }

    [HttpGet("{id}/fields")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCustomFields(Guid id)
    {
        var response = await inventoryService.GetCustomFieldsAsync(id);
        return Ok(ApiResponse.Ok(response));
    }

    [HttpPut("{id}/fields")]
    [Authorize(Policy = "Authenticated")]
    public async Task<IActionResult> UpdateCustomFields(Guid id, [FromBody] UpdateInventoryCustomFieldsRequest request)
    {
        var inventory = await dbContext.Inventories.FindAsync(id);
        if (inventory is null)
            return NotFound(ApiResponse.Fail(404, "Inventory was not found.", ErrorCodes.InventoryNotFound));

        var auth = await authorizationService.AuthorizeAsync(User, inventory, "OwnerOrAdmin");
        if (!auth.Succeeded)
            return Forbid();

        var response = await inventoryService.UpdateCustomFieldsAsync(id, request);
        return Ok(ApiResponse.Ok(response));
    }

    [HttpPut("{id}/settings")]
    [Authorize(Policy = "Authenticated")]
    public async Task<IActionResult> UpdateSettings(Guid id, [FromBody] UpdateInventorySettingsRequest request)
    {
        var inventory = await dbContext.Inventories.FindAsync(id);
        if (inventory is null)
            return NotFound(ApiResponse.Fail(404, "Inventory was not found.", ErrorCodes.InventoryNotFound));

        var auth = await authorizationService.AuthorizeAsync(User, inventory, "OwnerOrAdmin");
        if (!auth.Succeeded)
            return Forbid();

        var dto = await inventoryService.UpdateSettingsAsync(id, request);
        return Ok(ApiResponse.Ok(dto));
    }

    [HttpPost("{id}/access/add")]
    [Authorize(Policy = "Authenticated")]
    public async Task<IActionResult> AddAccess(Guid id, [FromBody] UpdateInventoryAccessRequest request)
    {
        var inventory = await dbContext.Inventories.FindAsync(id);
        if (inventory is null)
            return NotFound(ApiResponse.Fail(404, "Inventory was not found.", ErrorCodes.InventoryNotFound));

        var auth = await authorizationService.AuthorizeAsync(User, inventory, "OwnerOrAdmin");
        if (!auth.Succeeded)
            return Forbid();

        var response = await inventoryService.AddAccessAsync(id, request);
        return Ok(ApiResponse.Ok(response));
    }

    [HttpPost("{id}/access/remove")]
    [Authorize(Policy = "Authenticated")]
    public async Task<IActionResult> RemoveAccess(Guid id, [FromBody] UpdateInventoryAccessRequest request)
    {
        var inventory = await dbContext.Inventories.FindAsync(id);
        if (inventory is null)
            return NotFound(ApiResponse.Fail(404, "Inventory was not found.", ErrorCodes.InventoryNotFound));

        var auth = await authorizationService.AuthorizeAsync(User, inventory, "OwnerOrAdmin");
        if (!auth.Succeeded)
            return Forbid();

        var response = await inventoryService.RemoveAccessAsync(id, request);
        return Ok(ApiResponse.Ok(response));
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "Authenticated")]
    public async Task<IActionResult> DeleteInventory(Guid id)
    {
        var inventory = await dbContext.Inventories.FindAsync(id);
        if (inventory is null)
            return NotFound(ApiResponse.Fail(404, "Inventory was not found.", ErrorCodes.InventoryNotFound));

        var auth = await authorizationService.AuthorizeAsync(User, inventory, "OwnerOrAdmin");
        if (!auth.Succeeded)
            return Forbid();

        await inventoryService.DeleteAsync(id);
        return Ok(ApiResponse.Ok<object>(null!));
    }
}