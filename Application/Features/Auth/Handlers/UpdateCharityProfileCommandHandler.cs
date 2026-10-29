using EcoBridge.Application.Contracts;
using EcoBridge.Application.DTO.Users.Charity;
using EcoBridge.Application.DTO.Users.Volunteer;
using EcoBridge.Application.Features.Auth.Commands.Profiles;
using EcoBridge.Application.Result;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace EcoBridge.Application.Features.Auth.Handlers
{
    public class UpdateCharityProfileCommandHandler(IEcoBridgeDbContext dbContext,
    ICacheService cacheService,
    ICloudinaryService cloudinaryService) : IRequestHandler<UpdateCharityProfileCommand,ApiResult<CharityProfileDTO>>
    {

        public async Task<ApiResult<CharityProfileDTO>> Handle(UpdateCharityProfileCommand command, CancellationToken ct)
        {

            var account = await dbContext.Accounts
           .FirstOrDefaultAsync(x => x.Id == command.UserId, ct);

            if (account is null)
                throw new DomainException.NotFoundException(AppMessageCodes.USER_NOT_FOUND);

            // 1. Upload new profile image (if provided)
            if (command.updateCharityProfileDTO.ProfileImage is { Length: > 0 })
            {
                var uploadResult = await cloudinaryService
                    .AddImageAsync(command.updateCharityProfileDTO.ProfileImage);

                if (uploadResult is null || string.IsNullOrEmpty(uploadResult.Url))
                    throw new DomainException.FailureException(AppMessageCodes.UPLOAD_FAILED);

                account.UpdateProfileImage(uploadResult.Url);
            }

            // 2. Update basic profile fields
            account.UpdateProfile(
                command.updateCharityProfileDTO.FullName.Trim(),
                command.updateCharityProfileDTO.Phone.Trim());

            // 3. organization info (for charity)
            if (command.updateCharityProfileDTO.OrganizationName != null || command.updateCharityProfileDTO.RegistrationNumber != null)
            {
                account.SetOrganizationInfo(
                    command.updateCharityProfileDTO.OrganizationName?.Trim(),
                    command.updateCharityProfileDTO.RegistrationNumber?.Trim() ?? account.VehicleDetails);
            }

            await dbContext.SaveChangesAsync(ct);

            // 5. Invalidate Redis profile caches
            await cacheService.RemoveAsync($"user_profile_{command.UserId}", ct);
            await cacheService.RemoveAsync($"user:{command.UserId}", ct);

            // 6. Build DTO response
            var effectiveAddress = account.AddressDetails;

            var profile = new CharityProfileDTO
            {
                Id = account.Id,
                FullName = account.FullName,
                Email = account.Email ?? string.Empty,
                Phone = account.PhoneNumber ?? string.Empty,
                UserType = account.UserType.ToString(),
                TotalWalletPoints = account.TotalWalletPoints,
                TrustScore = account.TrustScore,
                RegistrationNumber = account.RegistrationNumber,
                OrganizationName = account.OrganizationName,
                CityId = account.CityId,
                Address = effectiveAddress,
                AddressDetails = effectiveAddress,
                ImagePath = account.ImagePath,
                CreatedAt = account.CreatedAt
            };

            return ApiResult<CharityProfileDTO>.Ok(profile, AppMessageCodes.PROFILE_UPDATED);

        }
        
    }
}
