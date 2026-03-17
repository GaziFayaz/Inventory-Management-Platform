using Inventory_Management_Platform.Common.Errors;
using Inventory_Management_Platform.Contracts.Inventory;
using Inventory_Management_Platform.Data;
using Inventory_Management_Platform.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Inventory_Management_Platform.Features.Inventory;

public sealed class InventoryService(
    AppDbContext dbContext,
    UserManager<AppUser> userManager) : IInventoryService
{
    public async Task<InventoryDto> CreateAsync(string ownerId, CreateInventoryRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            throw new AppException(400, "Title is required.", ErrorCodes.InventoryInvalidTitle);

        if (request.CategoryId.HasValue)
        {
            var categoryExists = await dbContext.Categories
                .AnyAsync(c => c.Id == request.CategoryId);

            if (!categoryExists)
                throw new AppException(400, "Category was not found.", ErrorCodes.InventoryCategoryNotFound);
        }

        var now = DateTime.UtcNow;
        var inventory = new Models.Inventory
        {
            Id = Guid.NewGuid(),
            OwnerId = ownerId,
            Title = request.Title,
            DescriptionMd = request.DescriptionMd,
            ImageUrl = request.ImageUrl,
            CategoryId = request.CategoryId,
            IsPublic = request.IsPublic,
            CreatedAt = now
        };

        var tags = await ResolveTagsAsync(request.TagNames);
        foreach (var tag in tags)
            inventory.Tags.Add(tag);

        dbContext.Inventories.Add(inventory);
        await dbContext.SaveChangesAsync();

        return await BuildInventoryDtoAsync(inventory.Id);
    }

    public async Task<InventoryListResponse> ListAsync(string? ownerId, int page, int pageSize)
    {
        var query = dbContext.Inventories
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(ownerId))
            query = query.Where(i => i.OwnerId == ownerId);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(i => i.UpdatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(i => new InventoryDto(
                i.Id,
                i.OwnerId,
                i.Owner.DisplayName,
                i.Title,
                i.DescriptionMd,
                i.ImageUrl,
                i.CategoryId,
                i.Category != null ? i.Category.Name : null,
                i.IsPublic,
                i.Tags.OrderBy(t => t.Name).Select(t => t.Name).ToList(),
                i.CreatedAt,
                i.UpdatedAt,
                EF.Property<uint>(i, "xmin")))
            .ToListAsync();

        return new InventoryListResponse(items, page, pageSize, totalCount);
    }

    public async Task<InventoryDto> GetByIdAsync(Guid id)
    {
        var dto = await dbContext.Inventories
            .AsNoTracking()
            .Where(i => i.Id == id)
            .Select(i => new InventoryDto(
                i.Id,
                i.OwnerId,
                i.Owner.DisplayName,
                i.Title,
                i.DescriptionMd,
                i.ImageUrl,
                i.CategoryId,
                i.Category != null ? i.Category.Name : null,
                i.IsPublic,
                i.Tags.OrderBy(t => t.Name).Select(t => t.Name).ToList(),
                i.CreatedAt,
                i.UpdatedAt,
                EF.Property<uint>(i, "xmin")))
            .SingleOrDefaultAsync();

        if (dto is null)
            throw new AppException(404, "Inventory was not found.", ErrorCodes.InventoryNotFound);

        return dto;
    }

    public async Task<InventoryAccessResponse> GetAccessAsync(Guid id)
    {
        var exists = await dbContext.Inventories
            .AnyAsync(i => i.Id == id);

        if (!exists)
            throw new AppException(404, "Inventory was not found.", ErrorCodes.InventoryNotFound);

        return await BuildAccessResponseAsync(id);
    }

    public async Task<InventoryCustomFieldsResponse> GetCustomFieldsAsync(Guid id)
    {
        var inventory = await dbContext.Inventories
            .AsNoTracking()
            .SingleOrDefaultAsync(i => i.Id == id);

        if (inventory is null)
            throw new AppException(404, "Inventory was not found.", ErrorCodes.InventoryNotFound);

        var version = await dbContext.Inventories
            .Where(i => i.Id == id)
            .Select(i => EF.Property<uint>(i, "xmin"))
            .SingleAsync();

        return new InventoryCustomFieldsResponse(id, BuildCustomFields(inventory), version);
    }

    public async Task<InventoryCustomFieldsResponse> UpdateCustomFieldsAsync(Guid id, UpdateInventoryCustomFieldsRequest request)
    {
        var inventory = await dbContext.Inventories
            .SingleOrDefaultAsync(i => i.Id == id);

        if (inventory is null)
            throw new AppException(404, "Inventory was not found.", ErrorCodes.InventoryNotFound);

        dbContext.Entry(inventory).Property("xmin").OriginalValue = request.Version;

        var fields = request.Fields ?? [];
        ValidateCustomFields(fields);

        ResetCustomFields(inventory);
        foreach (var field in fields)
            ApplyCustomField(inventory, field);

        inventory.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();

        var version = await dbContext.Inventories
            .Where(i => i.Id == id)
            .Select(i => EF.Property<uint>(i, "xmin"))
            .SingleAsync();

        return new InventoryCustomFieldsResponse(id, BuildCustomFields(inventory), version);
    }

    public async Task<InventoryDto> UpdateSettingsAsync(Guid id, UpdateInventorySettingsRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            throw new AppException(400, "Title is required.", ErrorCodes.InventoryInvalidTitle);

        if (request.CategoryId.HasValue)
        {
            var categoryExists = await dbContext.Categories
                .AnyAsync(c => c.Id == request.CategoryId.Value);

            if (!categoryExists)
                throw new AppException(400, "Category was not found.", ErrorCodes.InventoryCategoryNotFound);
        }

        var inventory = await dbContext.Inventories
            .Include(i => i.Tags)
            .SingleOrDefaultAsync(i => i.Id == id);

        if (inventory is null)
            throw new AppException(404, "Inventory was not found.", ErrorCodes.InventoryNotFound);

        dbContext.Entry(inventory).Property("xmin").OriginalValue = request.Version;

        inventory.Title = request.Title.Trim();
        inventory.DescriptionMd = request.DescriptionMd;
        inventory.ImageUrl = request.ImageUrl;
        inventory.CategoryId = request.CategoryId;
        inventory.IsPublic = request.IsPublic;
        inventory.UpdatedAt = DateTime.UtcNow;

        var tags = await ResolveTagsAsync(request.TagNames);
        inventory.Tags.Clear();
        foreach (var tag in tags)
            inventory.Tags.Add(tag);

        await dbContext.SaveChangesAsync();

        return await BuildInventoryDtoAsync(id);
    }

    public async Task<InventoryAccessResponse> AddAccessAsync(Guid id, UpdateInventoryAccessRequest request)
    {
        var inventory = await dbContext.Inventories
            .SingleOrDefaultAsync(i => i.Id == id);

        if (inventory is null)
            throw new AppException(404, "Inventory was not found.", ErrorCodes.InventoryNotFound);

        dbContext.Entry(inventory).Property("xmin").OriginalValue = request.Version;

        var users = await ResolveUsersByEmailsAsync(request.Emails);
        var userIds = users.Select(u => u.Id).ToList();

        var existingUserIds = await dbContext.InventoryAccesses
            .Where(a => a.InventoryId == id && userIds.Contains(a.UserId))
            .Select(a => a.UserId)
            .ToHashSetAsync();

        var missingUserIds = userIds
            .Where(userId => !existingUserIds.Contains(userId))
            .ToList();

        if (missingUserIds.Count > 0)
        {
            var rowsToAdd = missingUserIds
                .Select(userId => new InventoryAccess
                {
                    InventoryId = id,
                    UserId = userId
                });

            await dbContext.InventoryAccesses.AddRangeAsync(rowsToAdd);
        }

        inventory.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();

        return await BuildAccessResponseAsync(id);
    }

    public async Task<InventoryAccessResponse> RemoveAccessAsync(Guid id, UpdateInventoryAccessRequest request)
    {
        var inventory = await dbContext.Inventories
            .SingleOrDefaultAsync(i => i.Id == id);

        if (inventory is null)
            throw new AppException(404, "Inventory was not found.", ErrorCodes.InventoryNotFound);

        dbContext.Entry(inventory).Property("xmin").OriginalValue = request.Version;

        var users = await ResolveUsersByEmailsAsync(request.Emails);
        var userIds = users.Select(u => u.Id).ToList();

        var rowsToRemove = await dbContext.InventoryAccesses
            .Where(a => a.InventoryId == id && userIds.Contains(a.UserId))
            .ToListAsync();

        if (rowsToRemove.Count > 0)
            dbContext.InventoryAccesses.RemoveRange(rowsToRemove);

        inventory.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();

        return await BuildAccessResponseAsync(id);
    }

    public async Task DeleteAsync(Guid id)
    {
        var inventory = await dbContext.Inventories
            .SingleOrDefaultAsync(i => i.Id == id);

        if (inventory is null)
            throw new AppException(404, "Inventory was not found.", ErrorCodes.InventoryNotFound);

        dbContext.Inventories.Remove(inventory);
        await dbContext.SaveChangesAsync();
    }

    private async Task<InventoryDto> BuildInventoryDtoAsync(Guid id)
    {
        var dto = await dbContext.Inventories
            .AsNoTracking()
            .Where(i => i.Id == id)
            .Select(i => new InventoryDto(
                i.Id,
                i.OwnerId,
                i.Owner.DisplayName,
                i.Title,
                i.DescriptionMd,
                i.ImageUrl,
                i.CategoryId,
                i.Category != null ? i.Category.Name : null,
                i.IsPublic,
                i.Tags.OrderBy(t => t.Name).Select(t => t.Name).ToList(),
                i.CreatedAt,
                i.UpdatedAt,
                EF.Property<uint>(i, "xmin")))
            .SingleAsync();

        return dto;
    }

    private async Task<List<Tag>> ResolveTagsAsync(List<string>? rawTagNames)
    {
        var names = NormalizeTags(rawTagNames);
        if (names.Count == 0)
            return [];

        var existingTags = await dbContext.Tags
            .Where(t => names.Contains(t.Name))
            .ToListAsync();

        var existingSet = existingTags
            .Select(t => t.Name)
            .ToHashSet();

        var tagsToCreate = names
            .Where(name => !existingSet.Contains(name))
            .Select(name => new Tag { Name = name })
            .ToList();

        if (tagsToCreate.Count > 0)
            await dbContext.Tags.AddRangeAsync(tagsToCreate);

        return existingTags.Concat(tagsToCreate).ToList();
    }

    private static List<string> NormalizeTags(List<string>? rawTagNames)
    {
        if (rawTagNames is null || rawTagNames.Count == 0)
            return [];

        return rawTagNames
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Select(t => t.Trim().ToLowerInvariant())
            .Distinct()
            .ToList();
    }

    private async Task<List<AppUser>> ResolveUsersByEmailsAsync(List<string> emails)
    {
        var normalizedEmails = emails
            .Where(e => !string.IsNullOrWhiteSpace(e))
            .Select(e => e.Trim().ToUpperInvariant())
            .Distinct()
            .ToList();

        if (normalizedEmails.Count == 0)
            return [];

        var users = await userManager.Users
            .Where(u => u.NormalizedEmail != null && normalizedEmails.Contains(u.NormalizedEmail))
            .ToListAsync();

        if (users.Count != normalizedEmails.Count)
            throw new AppException(404, "One or more users were not found.", ErrorCodes.InventoryAccessUserNotFound);

        return users;
    }

    private async Task<InventoryAccessResponse> BuildAccessResponseAsync(Guid inventoryId)
    {
        var emails = await dbContext.InventoryAccesses
            .Where(a => a.InventoryId == inventoryId)
            .Join(
                userManager.Users,
                access => access.UserId,
                user => user.Id,
                (access, user) => user.Email)
            .Where(email => email != null)
            .Select(email => email!)
            .OrderBy(email => email)
            .ToListAsync();

        var version = await dbContext.Inventories
            .Where(i => i.Id == inventoryId)
            .Select(i => EF.Property<uint>(i, "xmin"))
            .SingleAsync();

        return new InventoryAccessResponse(inventoryId, emails, version);
    }

    private static void ValidateCustomFields(List<CustomFieldDto> fields)
    {
        var duplicateTypeSlot = fields
            .GroupBy(f => new { f.Type, f.Slot })
            .Any(g => g.Count() > 1);

        if (duplicateTypeSlot)
            throw new AppException(400, "Duplicate custom field slot detected.");

        var invalidSlot = fields.Any(f => f.Slot is < 1 or > 3);
        if (invalidSlot)
            throw new AppException(400, "Custom field slot must be between 1 and 3.");
    }

    private static List<CustomFieldDto> BuildCustomFields(Models.Inventory inventory)
    {
        return
        [
            new(CustomFieldType.String, 1, inventory.String1Enabled, inventory.String1Name, inventory.String1Description, inventory.String1ShowInTable, inventory.String1OrderIndex),
            new(CustomFieldType.String, 2, inventory.String2Enabled, inventory.String2Name, inventory.String2Description, inventory.String2ShowInTable, inventory.String2OrderIndex),
            new(CustomFieldType.String, 3, inventory.String3Enabled, inventory.String3Name, inventory.String3Description, inventory.String3ShowInTable, inventory.String3OrderIndex),

            new(CustomFieldType.MultiLine, 1, inventory.MultiLine1Enabled, inventory.MultiLine1Name, inventory.MultiLine1Description, inventory.MultiLine1ShowInTable, inventory.MultiLine1OrderIndex),
            new(CustomFieldType.MultiLine, 2, inventory.MultiLine2Enabled, inventory.MultiLine2Name, inventory.MultiLine2Description, inventory.MultiLine2ShowInTable, inventory.MultiLine2OrderIndex),
            new(CustomFieldType.MultiLine, 3, inventory.MultiLine3Enabled, inventory.MultiLine3Name, inventory.MultiLine3Description, inventory.MultiLine3ShowInTable, inventory.MultiLine3OrderIndex),

            new(CustomFieldType.Numeric, 1, inventory.Numeric1Enabled, inventory.Numeric1Name, inventory.Numeric1Description, inventory.Numeric1ShowInTable, inventory.Numeric1OrderIndex),
            new(CustomFieldType.Numeric, 2, inventory.Numeric2Enabled, inventory.Numeric2Name, inventory.Numeric2Description, inventory.Numeric2ShowInTable, inventory.Numeric2OrderIndex),
            new(CustomFieldType.Numeric, 3, inventory.Numeric3Enabled, inventory.Numeric3Name, inventory.Numeric3Description, inventory.Numeric3ShowInTable, inventory.Numeric3OrderIndex),

            new(CustomFieldType.Link, 1, inventory.Link1Enabled, inventory.Link1Name, inventory.Link1Description, inventory.Link1ShowInTable, inventory.Link1OrderIndex),
            new(CustomFieldType.Link, 2, inventory.Link2Enabled, inventory.Link2Name, inventory.Link2Description, inventory.Link2ShowInTable, inventory.Link2OrderIndex),
            new(CustomFieldType.Link, 3, inventory.Link3Enabled, inventory.Link3Name, inventory.Link3Description, inventory.Link3ShowInTable, inventory.Link3OrderIndex),

            new(CustomFieldType.Bool, 1, inventory.Bool1Enabled, inventory.Bool1Name, inventory.Bool1Description, inventory.Bool1ShowInTable, inventory.Bool1OrderIndex),
            new(CustomFieldType.Bool, 2, inventory.Bool2Enabled, inventory.Bool2Name, inventory.Bool2Description, inventory.Bool2ShowInTable, inventory.Bool2OrderIndex),
            new(CustomFieldType.Bool, 3, inventory.Bool3Enabled, inventory.Bool3Name, inventory.Bool3Description, inventory.Bool3ShowInTable, inventory.Bool3OrderIndex)
        ];
    }

    private static void ResetCustomFields(Models.Inventory inventory)
    {
        inventory.String1Enabled = false;
        inventory.String1Name = null;
        inventory.String1Description = null;
        inventory.String1ShowInTable = false;
        inventory.String1OrderIndex = 0;
        inventory.String2Enabled = false;
        inventory.String2Name = null;
        inventory.String2Description = null;
        inventory.String2ShowInTable = false;
        inventory.String2OrderIndex = 0;
        inventory.String3Enabled = false;
        inventory.String3Name = null;
        inventory.String3Description = null;
        inventory.String3ShowInTable = false;
        inventory.String3OrderIndex = 0;

        inventory.MultiLine1Enabled = false;
        inventory.MultiLine1Name = null;
        inventory.MultiLine1Description = null;
        inventory.MultiLine1ShowInTable = false;
        inventory.MultiLine1OrderIndex = 0;
        inventory.MultiLine2Enabled = false;
        inventory.MultiLine2Name = null;
        inventory.MultiLine2Description = null;
        inventory.MultiLine2ShowInTable = false;
        inventory.MultiLine2OrderIndex = 0;
        inventory.MultiLine3Enabled = false;
        inventory.MultiLine3Name = null;
        inventory.MultiLine3Description = null;
        inventory.MultiLine3ShowInTable = false;
        inventory.MultiLine3OrderIndex = 0;

        inventory.Numeric1Enabled = false;
        inventory.Numeric1Name = null;
        inventory.Numeric1Description = null;
        inventory.Numeric1ShowInTable = false;
        inventory.Numeric1OrderIndex = 0;
        inventory.Numeric2Enabled = false;
        inventory.Numeric2Name = null;
        inventory.Numeric2Description = null;
        inventory.Numeric2ShowInTable = false;
        inventory.Numeric2OrderIndex = 0;
        inventory.Numeric3Enabled = false;
        inventory.Numeric3Name = null;
        inventory.Numeric3Description = null;
        inventory.Numeric3ShowInTable = false;
        inventory.Numeric3OrderIndex = 0;

        inventory.Link1Enabled = false;
        inventory.Link1Name = null;
        inventory.Link1Description = null;
        inventory.Link1ShowInTable = false;
        inventory.Link1OrderIndex = 0;
        inventory.Link2Enabled = false;
        inventory.Link2Name = null;
        inventory.Link2Description = null;
        inventory.Link2ShowInTable = false;
        inventory.Link2OrderIndex = 0;
        inventory.Link3Enabled = false;
        inventory.Link3Name = null;
        inventory.Link3Description = null;
        inventory.Link3ShowInTable = false;
        inventory.Link3OrderIndex = 0;

        inventory.Bool1Enabled = false;
        inventory.Bool1Name = null;
        inventory.Bool1Description = null;
        inventory.Bool1ShowInTable = false;
        inventory.Bool1OrderIndex = 0;
        inventory.Bool2Enabled = false;
        inventory.Bool2Name = null;
        inventory.Bool2Description = null;
        inventory.Bool2ShowInTable = false;
        inventory.Bool2OrderIndex = 0;
        inventory.Bool3Enabled = false;
        inventory.Bool3Name = null;
        inventory.Bool3Description = null;
        inventory.Bool3ShowInTable = false;
        inventory.Bool3OrderIndex = 0;
    }

    private static void ApplyCustomField(Models.Inventory inventory, CustomFieldDto field)
    {
        switch (field.Type, field.Slot)
        {
            case (CustomFieldType.String, 1):
                inventory.String1Enabled = field.Enabled;
                inventory.String1Name = field.Title;
                inventory.String1Description = field.Description;
                inventory.String1ShowInTable = field.ShowInTable;
                inventory.String1OrderIndex = field.OrderIndex;
                break;
            case (CustomFieldType.String, 2):
                inventory.String2Enabled = field.Enabled;
                inventory.String2Name = field.Title;
                inventory.String2Description = field.Description;
                inventory.String2ShowInTable = field.ShowInTable;
                inventory.String2OrderIndex = field.OrderIndex;
                break;
            case (CustomFieldType.String, 3):
                inventory.String3Enabled = field.Enabled;
                inventory.String3Name = field.Title;
                inventory.String3Description = field.Description;
                inventory.String3ShowInTable = field.ShowInTable;
                inventory.String3OrderIndex = field.OrderIndex;
                break;

            case (CustomFieldType.MultiLine, 1):
                inventory.MultiLine1Enabled = field.Enabled;
                inventory.MultiLine1Name = field.Title;
                inventory.MultiLine1Description = field.Description;
                inventory.MultiLine1ShowInTable = field.ShowInTable;
                inventory.MultiLine1OrderIndex = field.OrderIndex;
                break;
            case (CustomFieldType.MultiLine, 2):
                inventory.MultiLine2Enabled = field.Enabled;
                inventory.MultiLine2Name = field.Title;
                inventory.MultiLine2Description = field.Description;
                inventory.MultiLine2ShowInTable = field.ShowInTable;
                inventory.MultiLine2OrderIndex = field.OrderIndex;
                break;
            case (CustomFieldType.MultiLine, 3):
                inventory.MultiLine3Enabled = field.Enabled;
                inventory.MultiLine3Name = field.Title;
                inventory.MultiLine3Description = field.Description;
                inventory.MultiLine3ShowInTable = field.ShowInTable;
                inventory.MultiLine3OrderIndex = field.OrderIndex;
                break;

            case (CustomFieldType.Numeric, 1):
                inventory.Numeric1Enabled = field.Enabled;
                inventory.Numeric1Name = field.Title;
                inventory.Numeric1Description = field.Description;
                inventory.Numeric1ShowInTable = field.ShowInTable;
                inventory.Numeric1OrderIndex = field.OrderIndex;
                break;
            case (CustomFieldType.Numeric, 2):
                inventory.Numeric2Enabled = field.Enabled;
                inventory.Numeric2Name = field.Title;
                inventory.Numeric2Description = field.Description;
                inventory.Numeric2ShowInTable = field.ShowInTable;
                inventory.Numeric2OrderIndex = field.OrderIndex;
                break;
            case (CustomFieldType.Numeric, 3):
                inventory.Numeric3Enabled = field.Enabled;
                inventory.Numeric3Name = field.Title;
                inventory.Numeric3Description = field.Description;
                inventory.Numeric3ShowInTable = field.ShowInTable;
                inventory.Numeric3OrderIndex = field.OrderIndex;
                break;

            case (CustomFieldType.Link, 1):
                inventory.Link1Enabled = field.Enabled;
                inventory.Link1Name = field.Title;
                inventory.Link1Description = field.Description;
                inventory.Link1ShowInTable = field.ShowInTable;
                inventory.Link1OrderIndex = field.OrderIndex;
                break;
            case (CustomFieldType.Link, 2):
                inventory.Link2Enabled = field.Enabled;
                inventory.Link2Name = field.Title;
                inventory.Link2Description = field.Description;
                inventory.Link2ShowInTable = field.ShowInTable;
                inventory.Link2OrderIndex = field.OrderIndex;
                break;
            case (CustomFieldType.Link, 3):
                inventory.Link3Enabled = field.Enabled;
                inventory.Link3Name = field.Title;
                inventory.Link3Description = field.Description;
                inventory.Link3ShowInTable = field.ShowInTable;
                inventory.Link3OrderIndex = field.OrderIndex;
                break;

            case (CustomFieldType.Bool, 1):
                inventory.Bool1Enabled = field.Enabled;
                inventory.Bool1Name = field.Title;
                inventory.Bool1Description = field.Description;
                inventory.Bool1ShowInTable = field.ShowInTable;
                inventory.Bool1OrderIndex = field.OrderIndex;
                break;
            case (CustomFieldType.Bool, 2):
                inventory.Bool2Enabled = field.Enabled;
                inventory.Bool2Name = field.Title;
                inventory.Bool2Description = field.Description;
                inventory.Bool2ShowInTable = field.ShowInTable;
                inventory.Bool2OrderIndex = field.OrderIndex;
                break;
            case (CustomFieldType.Bool, 3):
                inventory.Bool3Enabled = field.Enabled;
                inventory.Bool3Name = field.Title;
                inventory.Bool3Description = field.Description;
                inventory.Bool3ShowInTable = field.ShowInTable;
                inventory.Bool3OrderIndex = field.OrderIndex;
                break;
        }
    }
}