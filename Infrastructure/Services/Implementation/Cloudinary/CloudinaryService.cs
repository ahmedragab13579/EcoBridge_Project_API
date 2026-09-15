using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Results;
using EcoBridge.Infrastructure.Services.Constants;
using EcoBridge.Application.Contracts;
using EcoBridge.Infrastructure.Services.Photo;
using EcoBridge.Infrastructure.Services.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace EcoBridge.Infrastructure.Services.Implementation.Cloudinary;

public class CloudinaryService : ICloudinaryService
{
    private readonly CloudinaryDotNet.Cloudinary _cloudinary;

    public CloudinaryService(IOptions<CloudinarySettings> config)
    {
        var acc = new Account(
            config.Value.CloudName,
            config.Value.ApiKey,
            config.Value.ApiSecret
        );

        _cloudinary = new CloudinaryDotNet.Cloudinary(acc);
    }

    public async Task<Result<UploadImageResponse>> AddImageAsync(IFormFile file)
    {
        if (file == null)
        {
            return Result<UploadImageResponse>.Fail(AppMessageCodes.FILE_REQUIRED, ErrorType.Validation);
        }

        var validator = new CloudinaryFileValidator();
        var validationResult = validator.Validate(file);
        if (!validationResult.IsValid)
        {
            return Result<UploadImageResponse>.Fail(AppMessageCodes.UPLOAD_FAILED, ErrorType.Validation);
        }

        using var stream = file.OpenReadStream();
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

        var uploadResult = await _cloudinary.UploadAsync(uploadParams);

        if (uploadResult.Error != null)
        {
            return Result<UploadImageResponse>.Fail(uploadResult.Error.Message ?? AppMessageCodes.UPLOAD_FAILED, ErrorType.Failure);
        }

        var response = new UploadImageResponse
        {
            PublicId = uploadResult.PublicId,
            Url = uploadResult.SecureUrl?.ToString()
        };

        return Result<UploadImageResponse>.SuccessResult(response, AppMessageCodes.UPLOAD_SUCCESS);
    }

    public async Task<Result<UploadImageResponse>> UpdateImageAsync(IFormFile file, string publicId)
    {
        if (!string.IsNullOrEmpty(publicId))
        {
            await RemoveImageAsync(publicId);
        }

        var addResult = await AddImageAsync(file);

        if (addResult.IsSuccess && addResult.Data != null)
        {
            return Result<UploadImageResponse>.SuccessResult(addResult.Data, AppMessageCodes.UPDATE_SUCCESS);
        }

        return addResult;
    }

    public async Task<bool> RemoveImageAsync(string publicId)
    {
        if (string.IsNullOrEmpty(publicId))
            return false;

        var deleteParams = new DeletionParams(publicId);
        var result = await _cloudinary.DestroyAsync(deleteParams);

        return result.Result == "ok";
    }
}
