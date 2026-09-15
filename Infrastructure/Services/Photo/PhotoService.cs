using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using EcoBridge.Domain.Results;
using EcoBridge.Domain.Errors;
using EcoBridge.Application.Contracts;

namespace EcoBridge.Infrastructure.Services.Photo;

public class PhotoService : IPhotoService
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp"
    };

    private const long MaxFileSizeBytes = 5 * 1024 * 1024;
    private readonly Cloudinary? _cloudinary;

    public PhotoService(IOptions<CloudinarySettings> cloudinaryOptions)
    {
        var settings = cloudinaryOptions?.Value;
        if (settings is not null &&
            !string.IsNullOrWhiteSpace(settings.CloudName) &&
            !string.IsNullOrWhiteSpace(settings.ApiKey) &&
            !string.IsNullOrWhiteSpace(settings.ApiSecret))
        {
            var account = new Account(settings.CloudName, settings.ApiKey, settings.ApiSecret);
            _cloudinary = new Cloudinary(account);
            _cloudinary.Api.Secure = true;
        }
    }

    public bool IsEnabled => _cloudinary != null;

    public async Task<Result<string?>> UploadImageAsync(IFormFile file, CancellationToken ct = default)
    {
        if (!IsEnabled)
            return Result<string?>.FailResult(null, AppMessageCodes.UPLOAD_FAILED, ErrorType.Failure);

        if (file == null || file.Length == 0)
            return Result<string?>.FailResult(null, AppMessageCodes.FILE_REQUIRED, ErrorType.Validation);

        if (file.Length > MaxFileSizeBytes)
            return Result<string?>.FailResult(null, AppMessageCodes.FILE_TOO_LARGE, ErrorType.Validation);

        var extension = Path.GetExtension(file.FileName);
        if (string.IsNullOrWhiteSpace(extension) || !AllowedExtensions.Contains(extension))
            return Result<string?>.FailResult(null, AppMessageCodes.INVALID_FILE_TYPE, ErrorType.Validation);

        try
        {
            await using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = "ecobridge/donations"
            };

            var uploadResult = await _cloudinary!.UploadAsync(uploadParams, ct);
            if (uploadResult.Error is not null || uploadResult.SecureUrl is null)
                return Result<string?>.FailResult(null, AppMessageCodes.UPLOAD_FAILED, ErrorType.Failure);

            return Result<string?>.SuccessResult(uploadResult.SecureUrl.ToString(), AppMessageCodes.UPLOAD_SUCCESS);
        }
        catch (OperationCanceledException)
        {
            return Result<string?>.FailResult(null, AppMessageCodes.UPLOAD_FAILED, ErrorType.Failure);
        }
        catch (Exception)
        {
            return Result<string?>.FailResult(null, AppMessageCodes.UPLOAD_FAILED, ErrorType.Failure);
        }
    }
}
