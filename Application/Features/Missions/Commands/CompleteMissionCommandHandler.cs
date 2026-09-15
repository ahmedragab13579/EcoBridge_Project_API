using EcoBridge.Application.Contracts;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Results;
using EcoBridge.Domain.Errors;
using EcoBridge.Application.Features.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcoBridge.Application.Features.Missions.Commands;

public class CompleteMissionCommandHandler(
    IEcoBridgeDbContext dbContext,
    IPublisher publisher
) : IRequestHandler<CompleteMissionCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(CompleteMissionCommand request, CancellationToken cancellationToken)
    {
        var mission = await dbContext.DeliveryMissions
            .Include(m => m.DonationRequest)
                .ThenInclude(r => r.Donation)
            .FirstOrDefaultAsync(m => m.Id == request.MissionId, cancellationToken);

        if (mission == null)
            return Result<bool>.FailResult(false, AppMessageCodes.MISSION_NOT_FOUND, ErrorType.NotFound);

        if (mission.VolunteerId != request.VolunteerAccountId)
            return Result<bool>.FailResult(false, AppMessageCodes.FORBIDDEN, ErrorType.Forbidden);

        if (mission.MissionStatus == MissionStatus.Delivered)
            return Result<bool>.FailResult(false, AppMessageCodes.MISSION_INVALID_STATUS, ErrorType.Conflict);

        if (mission.MissionStatus != MissionStatus.InTransit && mission.MissionStatus != MissionStatus.Accepted)
            return Result<bool>.FailResult(false, AppMessageCodes.MISSION_INVALID_STATUS, ErrorType.Conflict);

        if (string.IsNullOrWhiteSpace(request.OtpCode) ||
            !string.Equals(mission.OtpCode?.Trim(), request.OtpCode.Trim(), StringComparison.Ordinal))
        {
            return Result<bool>.FailResult(false, AppMessageCodes.INVALID_OTP, ErrorType.Validation);
        }

        if (mission.OtpExpiresAtUtc.HasValue && mission.OtpExpiresAtUtc.Value < DateTime.UtcNow)
        {
            return Result<bool>.FailResult(false, AppMessageCodes.OTP_EXPIRED, ErrorType.Validation);
        }

        mission.MissionStatus = MissionStatus.Delivered;
        mission.DeliveryTime = DateTime.UtcNow;

        if (mission.DonationRequest?.Donation != null)
        {
            mission.DonationRequest.Donation.Status = DonationStatus.Delivered;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        // Publish event for wallet reward, SignalR notification, and history logging
        await publisher.Publish(new DeliveryCompletedEvent(
            mission.Id,
            request.VolunteerAccountId,
            mission.DonationRequest?.CharityId ?? Guid.Empty,
            mission.DonationRequest?.Donation?.DonorId ?? Guid.Empty
        ), cancellationToken);

        return Result<bool>.SuccessResult(true, AppMessageCodes.MISSION_COMPLETED);
    }
}
