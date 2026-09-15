using EcoBridge.Application.Contracts;
using EcoBridge.Application.DTO;
using EcoBridge.Application.Extensions;
using EcoBridge.Domain.Results;
using EcoBridge.Domain.Errors;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcoBridge.Application.Features.Auth.Queries;

public class GetUserProfileQueryHandler : IRequestHandler<GetUserProfileQuery, Result<UserProfileDTO>>
{
    private readonly IEcoBridgeDbContext _dbContext;

    public GetUserProfileQueryHandler(IEcoBridgeDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<UserProfileDTO>> Handle(GetUserProfileQuery query, CancellationToken ct)
    {
        var account = await _dbContext.Accounts
            .AsNoTracking()
            .Where(x => x.Id == query.UserId)
            .Select(x => new
            {
                x.Id,
                x.FullName,
                Email = x.Email ?? string.Empty,
                x.Phone,
                x.RoleId,
                x.UserType,
                x.TotalWalletPoints,
                x.TrustScore,
                x.CityId,
                VehicleType = x.Volunteer != null ? x.Volunteer.VehicleType : null,
                Address = x.AddressDetails ?? (x.Charity != null ? x.Charity.Address : (x.Donor != null ? x.Donor.Address : null)),
                AddressDetails = x.AddressDetails ?? (x.Charity != null ? x.Charity.Address : (x.Donor != null ? x.Donor.Address : null)),
                x.ImagePath,
                x.CreatedAt
            })
            .FirstOrDefaultAsync(ct);

        if (account is null)
        {
            return Result<UserProfileDTO>.FailResult(null!, ErrorCodes.USER_NOT_FOUND);
        }

        var profile = new UserProfileDTO
        {
            Id = account.Id,
            FullName = account.FullName,
            Email = account.Email,
            Phone = account.Phone,
            UserType = account.UserType.ToString(),
            TotalWalletPoints = account.TotalWalletPoints,
            TrustScore = account.TrustScore,
            CityId = account.CityId,
            VehicleType = account.VehicleType,
            Address = account.Address,
            AddressDetails = account.AddressDetails,
            ImagePath = account.ImagePath.ToCloudinaryUrl(),
            CreatedAt = account.CreatedAt
        };

        return Result<UserProfileDTO>.SuccessResult(profile, AppMessageCodes.PROFILE_RETRIEVED);
    }
}
