using Microsoft.AspNetCore.Http;

namespace Inventory_Management_Platform.Features.Inventory;

public interface IImageStorageService
{
    Task<string> UploadImageAsync(IFormFile file, CancellationToken cancellationToken = default);
}