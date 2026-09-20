using EcoBridge.Application.Contracts;
using EcoBridge.Application.Features.Missions.Queries;
using EcoBridge.Application.Result;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcoBridge.Application.Features.Missions.Handlers;

public class GetMissionOtpQueryHandler(
    IEcoBridgeDbContext dbContext
) : IRequestHandler<GetMissionOtpQuery, ApiResult<string>>
{
    public async Task<ApiResult<string>> Handle(
        GetMissionOtpQuery request,
        CancellationToken cancellationToken)
    {
        var mission = await dbContext.DeliveryMissions
            .AsNoTracking()
            .Where(m => m.Id == request.MissionId)
            .Select(m => new
            {
                CharityId = m.DonationId != Guid.Empty ? (Guid?)m.Donation.CharityId : null,
                m.Donation.Status,
                m.OtpCode
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (mission == null)
            throw new DomainException.NotFoundException(AppMessageCodes.MISSION_NOT_FOUND);

        if (mission.CharityId != request.CharityAccountId)
            throw new DomainException.ForbiddenException(AppMessageCodes.FORBIDDEN);

        if (mission.Status != DonationStatus.Accepted
            && mission.Status != DonationStatus.InTransit)
            throw new DomainException.ConflictException(AppMessageCodes.MISSION_INVALID_STATUS);

        if (string.IsNullOrWhiteSpace(mission.OtpCode))
            throw new DomainException.NotFoundException(AppMessageCodes.INVALID_OTP);

        return ApiResult<string>.Ok(mission.OtpCode, AppMessageCodes.MISSION_OTP_RETRIEVED);
    }
}