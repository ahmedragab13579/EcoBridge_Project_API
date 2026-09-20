using Microsoft.AspNetCore.Http;

namespace EcoBridge.Application.Contracts;

public interface ICloudinaryService
{
    Task<UploadImageResponse> AddImageAsync(IFormFile file, CancellationToken ct = default);
    Task<UploadImageResponse> UpdateImageAsync(IFormFile file, string? publicId, CancellationToken ct = default);
    Task RemoveImageAsync(string publicId, CancellationToken ct = default);
}