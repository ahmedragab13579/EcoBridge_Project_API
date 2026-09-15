using Microsoft.AspNetCore.Http;
using EcoBridge.Domain.Results;

namespace EcoBridge.Application.Contracts;

public interface IPhotoService
{
    Task<Result<string?>> UploadImageAsync(IFormFile file, CancellationToken ct = default);
    bool IsEnabled { get; }
}
