using EcoBridge.Application.Contracts;
using EcoBridge.Application.DTO.Users.Auth;
using EcoBridge.Application.DTO.Users.Auth.Users.Common;
using EcoBridge.Application.DTO.Users.Volunteer;
using EcoBridge.Application.Features.Auth.Commands.Profiles;
using EcoBridge.Application.Result;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcoBridge.Application.Features.Auth.Handlers;

public class UpdateVolunteerProfileCommandHandler(
    IEcoBridgeDbContext dbContext,
    ICacheService cacheService,
    ICloudinaryService cloudinaryService
) : IRequestHandler<UpdateVolunteerProfileCommand, ApiResult<VolunteerProfileDTO>>
{
    public async Task<ApiResult<VolunteerProfileDTO>> Handle(
        UpdateVolunteerProfileCommand command,
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

        await dbContext.SaveChangesAsync(ct);

        // 5. Invalidate Redis profile caches
        await cacheService.RemoveAsync($"user_profile_{command.UserId}", ct);
        await cacheService.RemoveAsync($"user:{command.UserId}", ct);

        // 6. Build DTO response
        var effectiveAddress = account.AddressDetails;

        var profile = new VolunteerProfileDTO
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

        return ApiResult<VolunteerProfileDTO>.Ok(profile, AppMessageCodes.PROFILE_UPDATED);
    }
}