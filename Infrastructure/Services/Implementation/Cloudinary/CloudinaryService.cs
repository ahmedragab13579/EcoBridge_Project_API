using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using EcoBridge.Application.Contracts;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Exceptions;
using EcoBridge.Infrastructure.Services.Constants;
using EcoBridge.Infrastructure.Services.Photo;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace EcoBridge.Infrastructure.Services.Implementation.Cloudinary;

public class CloudinaryService : ICloudinaryService
{
    private readonly CloudinaryDotNet.Cloudinary _cloudinary;

    public CloudinaryService(IOptions<CloudinarySettings> config)
    {
        var settings = config.Value;
        var acc = new Account(settings.CloudName, settings.ApiKey, settings.ApiSecret);
        _cloudinary = new CloudinaryDotNet.Cloudinary(acc)
        {
            Api = { Secure = true }
        };
    }

    public async Task<UploadImageResponse> AddImageAsync(IFormFile file, CancellationToken ct = default)
    {
        if (file is null || file.Length == 0)
            throw new DomainException.ValidationException(AppMessageCodes.FILE_REQUIRED);

        var validator = new CloudinaryFileValidator();
        var validationResult = validator.Validate(file);

        if (!validationResult.IsValid)
            throw new DomainException.ValidationException(AppMessageCodes.UPLOAD_FAILED);

        try
        {
            await using var stream = file.OpenReadStream();

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = AppCloudinaryConstants.FolderName,
                Transformation = new Transformation()
                    .Width(500)
                    .Height(500)
                    .Crop("fill")
                    .Quality("auto")
                    .FetchFormat("auto")
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams, ct);

            if (uploadResult.Error is not null)
                throw new DomainException.FailureException(AppMessageCodes.UPLOAD_FAILED);

            return new UploadImageResponse
            {
                PublicId = uploadResult.PublicId,
                Url = uploadResult.SecureUrl?.ToString()
            };
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (DomainException)
        {
            throw;
        }
        catch (Exception)
        {
            throw new DomainException.FailureException(AppMessageCodes.UPLOAD_FAILED);
        }
    }

    public async Task<UploadImageResponse> UpdateImageAsync(
        IFormFile file,
        string? publicId,
        CancellationToken ct = default)
    {
        // Best-effort delete of the old image
        if (!string.IsNullOrWhiteSpace(publicId))
        {
            try
            {
                await RemoveImageAsync(publicId, ct);
            }
            catch (DomainException)
            {
                // Old image not found or already deleted — ignore
            }
        }

        return await AddImageAsync(file, ct);
    }

    public async Task RemoveImageAsync(string publicId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(publicId))
            throw new DomainException.ValidationException(AppMessageCodes.PUBLIC_ID_REQUIRED);

        try
        {
            var deleteParams = new DeletionParams(publicId);
            var result = await _cloudinary.DestroyAsync(deleteParams);

            if (result.Result != "ok" && result.Result != "not found")
                throw new DomainException.FailureException(AppMessageCodes.DELETE_FAILED);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (DomainException)
        {
            throw;
        }
        catch (Exception)
        {
            throw new DomainException.FailureException(AppMessageCodes.DELETE_FAILED);
        }
    }
}