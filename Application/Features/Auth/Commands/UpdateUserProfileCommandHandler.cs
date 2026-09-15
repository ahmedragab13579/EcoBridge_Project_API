using EcoBridge.Application.Contracts;
using EcoBridge.Application.DTO;
using EcoBridge.Domain.Results;
using EcoBridge.Domain.Errors;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcoBridge.Application.Features.Auth.Commands;

public class UpdateUserProfileCommandHandler : IRequestHandler<UpdateUserProfileCommand, Result<UserProfileDTO>>
{
    private readonly IEcoBridgeDbContext _dbContext;
    private readonly ICacheService _cacheService;
    private readonly ICloudinaryService _cloudinaryService;

    public UpdateUserProfileCommandHandler(
        IEcoBridgeDbContext dbContext,
        ICacheService cacheService,
        ICloudinaryService cloudinaryService)
    {
        _dbContext = dbContext;
        _cacheService = cacheService;
        _cloudinaryService = cloudinaryService;
    }

    public async Task<Result<UserProfileDTO>> Handle(UpdateUserProfileCommand command, CancellationToken ct)
    {
        var account = await _dbContext.Accounts
            .Include(a => a.Volunteer)
            .Include(a => a.Charity)
            .Include(a => a.Donor)
            .FirstOrDefaultAsync(x => x.Id == command.UserId, ct);

        if (account is null)
        {
            return Result<UserProfileDTO>.FailResult(new UserProfileDTO(), AppMessageCodes.USER_NOT_FOUND, ErrorType.NotFound);
        }

        if (command.Request.ProfileImage != null && command.Request.ProfileImage.Length > 0)
        {
            var uploadResult = await _cloudinaryService.AddImageAsync(command.Request.ProfileImage);
            if (!uploadResult.IsSuccess)
            {
                return Result<UserProfileDTO>.FailResult(new UserProfileDTO(), uploadResult.Message, uploadResult.ErrorType);
            }

            if (uploadResult.Data != null && !string.IsNullOrEmpty(uploadResult.Data.Url))
            {
                account.UpdateProfileImage(uploadResult.Data.Url);
            }
        }

        var addressDetails = command.Request.AddressDetails ?? command.Request.Address;
        account.UpdateProfile(command.Request.FullName.Trim(), command.Request.Phone.Trim(), command.Request.CityId, addressDetails);

        if (account.Volunteer != null && command.Request.VehicleType != null)
        {
            account.Volunteer.UpdateVehicle(command.Request.VehicleType.Trim(), account.Volunteer.VehicleDetails);
        }

        if (account.Charity != null && command.Request.Address != null)
        {
            account.Charity.UpdateOrganization(account.Charity.OrganizationName, account.Charity.RegistrationNumber, command.Request.Address.Trim());
        }

        if (account.Donor != null && command.Request.Address != null)
        {
            account.Donor.UpdateDetails(account.Donor.OrganizationName, command.Request.Address.Trim());
        }

        await _dbContext.SaveChangesAsync(ct);

        // Invalidate Redis profile caches
        await _cacheService.RemoveAsync($"user_profile_{command.UserId}", ct);
        await _cacheService.RemoveAsync($"user:{command.UserId}", ct);

        var effectiveAddress = account.AddressDetails ?? account.Charity?.Address ?? account.Donor?.Address;

        var profile = new UserProfileDTO
        {
            Id = account.Id,
            FullName = account.FullName,
            Email = account.Email ?? string.Empty,
            Phone = account.Phone,
            UserType = account.UserType.ToString(),
            TotalWalletPoints = account.TotalWalletPoints,
            TrustScore = account.TrustScore,
            CityId = account.CityId,
            VehicleType = account.Volunteer?.VehicleType,
            Address = effectiveAddress,
            AddressDetails = effectiveAddress,
            ImagePath = account.ImagePath,
            CreatedAt = account.CreatedAt
        };

        return Result<UserProfileDTO>.SuccessResult(profile, AppMessageCodes.PROFILE_UPDATED);
    }
}
