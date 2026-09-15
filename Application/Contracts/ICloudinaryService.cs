using EcoBridge.Domain.Results;
using Microsoft.AspNetCore.Http;

namespace EcoBridge.Application.Contracts;

public interface ICloudinaryService
{
    Task<Result<UploadImageResponse>> AddImageAsync(IFormFile file);
    Task<Result<UploadImageResponse>> UpdateImageAsync(IFormFile file, string publicId);
    Task<bool> RemoveImageAsync(string publicId);
}
