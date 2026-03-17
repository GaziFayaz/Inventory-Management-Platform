using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Inventory_Management_Platform.Common.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Inventory_Management_Platform.Features.Inventory;

public sealed class CloudinaryImageStorageService(IOptions<CloudinaryOptions> options) : IImageStorageService
{
    private readonly Cloudinary _cloudinary = BuildClient(options.Value);
    private readonly string _folder = options.Value.Folder;

    public async Task<string> UploadImageAsync(IFormFile file, CancellationToken cancellationToken = default)
    {
        if (file.Length <= 0)
            throw new AppException(400, "Image file is empty.");

        await using var stream = file.OpenReadStream();

        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(file.FileName, stream),
            Folder = _folder
        };

        var uploadResult = await _cloudinary.UploadAsync(uploadParams, cancellationToken);

        if (uploadResult.Error is not null || uploadResult.SecureUrl is null)
            throw new AppException(500, "Failed to upload image.", ErrorCodes.ServerError);

        return uploadResult.SecureUrl.ToString();
    }

    private static Cloudinary BuildClient(CloudinaryOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.CloudName) ||
            string.IsNullOrWhiteSpace(options.ApiKey) ||
            string.IsNullOrWhiteSpace(options.ApiSecret))
            throw new InvalidOperationException("Cloudinary configuration is missing.");

        var account = new Account(options.CloudName, options.ApiKey, options.ApiSecret);
        var cloudinary = new Cloudinary(account);
        cloudinary.Api.Secure = true;

        return cloudinary;
    }
}