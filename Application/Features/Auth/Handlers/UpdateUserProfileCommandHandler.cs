using EcoBridge.Application.Contracts;
using EcoBridge.Application.DTO;
using EcoBridge.Application.Features.Auth.Commands;
using EcoBridge.Application.Result;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcoBridge.Application.Features.Auth.Handlers;

public class UpdateUserProfileCommandHandler(
    IEcoBridgeDbContext dbContext,
    ICacheService cacheService,
    ICloudinaryService cloudinaryService
) : IRequestHandler<UpdateUserProfileCommand, ApiResult<UserProfileDTO>>
{
    public async Task<ApiResult<UserProfileDTO>> Handle(
        UpdateUserProfileCommand command,
        CancellationToken ct)
    {
        var account = await dbContext.Accounts
            .FirstOrDefaultAsync(x => x.Id == command.UserId, ct);

        if (account is null)
            throw new DomainException.NotFoundException(AppMessageCodes.USER_NOT_FOUND);

        // 1. Upload new profile image (if provided)
        if (command.Request.ProfileImage is { Length: > 0 })
        {
            var uploadResult = await cloudinaryService
                .AddImageAsync(command.Request.ProfileImage);

            if (uploadResult is null || string.IsNullOrEmpty(uploadResult.Url))
                throw new DomainException.FailureException(AppMessageCodes.UPLOAD_FAILED);

            account.UpdateProfileImage(uploadResult.Url);
        }

        // 2. Update basic profile fields
        account.UpdateProfile(
            command.Request.FullName.Trim(),
            command.Request.Phone.Trim());

        // 3. Vehicle info (for volunteers)
        if (command.Request.VehicleType != null || command.Request.VehicleDetails != null)
        {
            account.SetVehicleInfo(
                command.Request.VehicleType?.Trim(),
                command.Request.VehicleDetails?.Trim() ?? account.VehicleDetails);
        }

        // 4. Organization info (for charities)
        if (command.Request.OrganizationName != null || command.Request.RegistrationNumber != null)
        {
            account.SetOrganizationInfo(
                command.Request.OrganizationName?.Trim() ?? account.OrganizationName,
                command.Request.RegistrationNumber?.Trim() ?? account.RegistrationNumber);
        }

        await dbContext.SaveChangesAsync(ct);

        // 5. Invalidate Redis profile caches
        await cacheService.RemoveAsync($"user_profile_{command.UserId}", ct);
        await cacheService.RemoveAsync($"user:{command.UserId}", ct);

        // 6. Build DTO response
        var effectiveAddress = account.AddressDetails;

        var profile = new UserProfileDTO
        {
            Id = account.Id,
            FullName = account.FullName,
            Email = account.Email ?? string.Empty,
            Phone = account.PhoneNumber ?? string.Empty,
            UserType = account.UserType.ToString(),
            TotalWalletPoints = account.TotalWalletPoints,
            TrustScore = account.TrustScore,
            CityId = account.CityId,
            VehicleType = account.VehicleType,
            Address = effectiveAddress,
            AddressDetails = effectiveAddress,
            ImagePath = account.ImagePath,
            CreatedAt = account.CreatedAt
        };

        return ApiResult<UserProfileDTO>.Ok(profile, AppMessageCodes.PROFILE_UPDATED);
    }
}