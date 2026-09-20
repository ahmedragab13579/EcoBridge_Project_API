using Microsoft.AspNetCore.Http;
using EcoBridge.Application.Results;

namespace EcoBridge.Application.Contracts;

public interface IPhotoService
{
    Task<string> UploadImageAsync(IFormFile file, CancellationToken ct = default);
    bool IsEnabled { get; }
}
