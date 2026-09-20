using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Exceptions;
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

    public async Task<string> UploadImageAsync(IFormFile file, CancellationToken ct = default)
    {
        // ─── Service availability ───
        if (!IsEnabled)
            throw new DomainException.FailureException(AppMessageCodes.UPLOAD_FAILED);

        // ─── Input validation ───
        if (file is null || file.Length == 0)
            throw new DomainException.ValidationException(AppMessageCodes.FILE_REQUIRED);

        if (file.Length > MaxFileSizeBytes)
            throw new DomainException.ValidationException(AppMessageCodes.FILE_TOO_LARGE);

        var extension = Path.GetExtension(file.FileName);
        if (string.IsNullOrWhiteSpace(extension) || !AllowedExtensions.Contains(extension))
            throw new DomainException.ValidationException(AppMessageCodes.INVALID_FILE_TYPE);

        // ─── Upload ───
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
                throw new DomainException.FailureException(AppMessageCodes.UPLOAD_FAILED);

            return uploadResult.SecureUrl.ToString();
        }
        catch (OperationCanceledException)
        {
            // Client disconnected — let it propagate untouched
            throw;
        }
        catch (DomainException)
        {
            // Already a domain exception — let it propagate
            throw;
        }
        catch (Exception)
        {
            // Wrap any unexpected infrastructure/network error
            throw new DomainException.FailureException(AppMessageCodes.UPLOAD_FAILED);
        }
    }
}