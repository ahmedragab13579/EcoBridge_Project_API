using EcoBridge.Application.Contracts;
using EcoBridge.Application.DTO.Users.Auth.Users.Common;
using EcoBridge.Application.DTO.Users.Volunteer;
using EcoBridge.Application.Features.Auth.Commands.Profiles;
using EcoBridge.Application.Result;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcoBridge.Application.Features.Auth.Handlers
{
    public class UpdateDonorAdminProfileCommandHandler(IEcoBridgeDbContext dbContext,
    ICacheService cacheService,
    ICloudinaryService cloudinaryService) : IRequestHandler<UpdateDonorAdminProfileCommand, ApiResult<UserDTO>>
    {
        public async Task<ApiResult<UserDTO>> Handle(UpdateDonorAdminProfileCommand command, CancellationToken ct)
        {
            var account = await dbContext.Accounts
                     .FirstOrDefaultAsync(x => x.Id == command.id, ct);

            if (account is null)
                throw new DomainException.NotFoundException(AppMessageCodes.USER_NOT_FOUND);

            // 1. Upload new profile image (if provided)
            if (command.request.ProfileImage is { Length: > 0 })
            {
                var uploadResult = await cloudinaryService
                    .AddImageAsync(command.request.ProfileImage);

                if (uploadResult is null || string.IsNullOrEmpty(uploadResult.Url))
                    throw new DomainException.FailureException(AppMessageCodes.UPLOAD_FAILED);

                account.UpdateProfileImage(uploadResult.Url);
            }

            // 2. Update basic profile fields
            account.UpdateProfile(
                command.request.FullName.Trim(),
                command.request.Phone.Trim());



            await dbContext.SaveChangesAsync(ct);

            // 5. Invalidate Redis profile caches
            await cacheService.RemoveAsync($"user_profile_{command.id}", ct);
            await cacheService.RemoveAsync($"user:{command.id}", ct);

            // 6. Build DTO response
            var effectiveAddress = account.AddressDetails;

            var profile = new UserDTO
            {
                Id = account.Id,
                FullName = account.FullName,
                Email = account.Email ?? string.Empty,
                Phone = account.PhoneNumber ?? string.Empty,
                UserType = account.UserType.ToString(),
                TotalWalletPoints = account.TotalWalletPoints,
                TrustScore = account.TrustScore,
                CityId = account.CityId,
                Address = effectiveAddress,
                AddressDetails = effectiveAddress,
                ImagePath = account.ImagePath,
                CreatedAt = account.CreatedAt
            };

            return ApiResult<UserDTO>.Ok(profile, AppMessageCodes.PROFILE_UPDATED);
        }
    }
}
