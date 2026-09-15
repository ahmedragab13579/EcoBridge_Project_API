using EcoBridge.Domain.Errors;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace EcoBridge.Infrastructure.Services.Implementation.Cloudinary;

public class CloudinaryFileValidator : AbstractValidator<IFormFile>
{
    public CloudinaryFileValidator()
    {
        RuleFor(x => x.Length)
            .GreaterThan(0).WithMessage(AppMessageCodes.FILE_REQUIRED.ToString())
            .LessThanOrEqualTo(5 * 1024 * 1024).WithMessage(AppMessageCodes.UPLOAD_FAILED.ToString());

        RuleFor(x => x.ContentType)
            .Must(contentType => contentType != null && 
                (contentType.Equals("image/jpeg", StringComparison.OrdinalIgnoreCase) ||
                 contentType.Equals("image/jpg", StringComparison.OrdinalIgnoreCase) ||
                 contentType.Equals("image/png", StringComparison.OrdinalIgnoreCase) ||
                 contentType.Equals("image/webp", StringComparison.OrdinalIgnoreCase)))
            .WithMessage(AppMessageCodes.UPLOAD_FAILED.ToString());
    }
}
