using EcoBridge.Application.Contracts;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Results;
using EcoBridge.Domain.Errors;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcoBridge.Application.Features.Missions.Queries;

public class GetMissionOtpQueryHandler(
    IEcoBridgeDbContext dbContext
) : IRequestHandler<GetMissionOtpQuery, Result<string>>
{
    public async Task<Result<string>> Handle(GetMissionOtpQuery request, CancellationToken cancellationToken)
    {
        var mission = await dbContext.DeliveryMissions
            .AsNoTracking()
            .Where(m => m.Id == request.MissionId)
            .Select(m => new
            {
                CharityId = m.DonationRequest != null ? (Guid?)m.DonationRequest.CharityId : null,
                m.MissionStatus,
                m.OtpCode
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (mission == null)
            return Result<string>.FailResult(string.Empty, AppMessageCodes.MISSION_NOT_FOUND, ErrorType.NotFound);

        if (mission.CharityId != request.CharityAccountId)
            return Result<string>.FailResult(string.Empty, AppMessageCodes.FORBIDDEN, ErrorType.Forbidden);

        if (mission.MissionStatus != MissionStatus.Accepted && mission.MissionStatus != MissionStatus.InTransit)
            return Result<string>.FailResult(string.Empty, AppMessageCodes.MISSION_INVALID_STATUS, ErrorType.Conflict);

        if (string.IsNullOrWhiteSpace(mission.OtpCode))
            return Result<string>.FailResult(string.Empty, AppMessageCodes.INVALID_OTP, ErrorType.NotFound);

        return Result<string>.SuccessResult(mission.OtpCode, AppMessageCodes.MISSION_OTP_RETRIEVED);
    }
}
