using Inventory_Management_Platform.Contracts.Inventory;

namespace Inventory_Management_Platform.Features.Inventory;

public interface IInventoryService
{
    Task<InventoryDto> CreateAsync(string ownerId, CreateInventoryRequest request);
    Task<InventoryListResponse> ListAsync(string? ownerId, int page, int pageSize);
    Task<InventoryDto> GetByIdAsync(Guid id);
    Task<InventoryAccessResponse> GetAccessAsync(Guid id);
    Task<InventoryCustomFieldsResponse> GetCustomFieldsAsync(Guid id);
    Task<InventoryCustomFieldsResponse> UpdateCustomFieldsAsync(Guid id, UpdateInventoryCustomFieldsRequest request);
    Task<InventoryDto> UpdateSettingsAsync(Guid id, UpdateInventorySettingsRequest request);
    Task<InventoryAccessResponse> AddAccessAsync(Guid id, UpdateInventoryAccessRequest request);
    Task<InventoryAccessResponse> RemoveAccessAsync(Guid id, UpdateInventoryAccessRequest request);
    Task DeleteAsync(Guid id);
}