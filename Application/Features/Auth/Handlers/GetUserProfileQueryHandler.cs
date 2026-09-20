using EcoBridge.Application.Contracts;
using EcoBridge.Application.DTO;
using EcoBridge.Application.Extensions;
using EcoBridge.Application.Features.Auth.Queries;
using EcoBridge.Application.Result;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcoBridge.Application.Features.Auth.Handlers;

public class GetUserProfileQueryHandler(
    IEcoBridgeDbContext dbContext
) : IRequestHandler<GetUserProfileQuery, ApiResult<UserProfileDTO>>
{
    public async Task<ApiResult<UserProfileDTO>> Handle(
        GetUserProfileQuery query,
        CancellationToken ct)
    {
        if (query.UserId == Guid.Empty)
            throw new DomainException.UnauthorizedException(AppMessageCodes.UNAUTHORIZED);

        var profile = await dbContext.Accounts
            .AsNoTracking()
            .Where(x => x.Id == query.UserId)
            .Select(x => new UserProfileDTO
            {
                Id = x.Id,
                FullName = x.FullName,
                Email = x.Email ?? string.Empty,
                Phone = x.PhoneNumber ?? string.Empty,
                UserType = x.UserType.ToString(),
                TotalWalletPoints = x.TotalWalletPoints,
                TrustScore = x.TrustScore,
                CityId = x.CityId,
                VehicleType = x.VehicleType,
                Address = x.AddressDetails,
                AddressDetails = x.AddressDetails,
                ImagePath = x.ImagePath.ToCloudinaryUrl(),
                CreatedAt = x.CreatedAt
            })
            .FirstOrDefaultAsync(ct);

        if (profile is null)
            throw new DomainException.NotFoundException(AppMessageCodes.USER_NOT_FOUND);

        return ApiResult<UserProfileDTO>.Ok(profile, AppMessageCodes.PROFILE_RETRIEVED);
    }
}