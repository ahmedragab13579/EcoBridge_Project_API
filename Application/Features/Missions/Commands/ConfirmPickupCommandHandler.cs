using EcoBridge.Application.Contracts;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Results;
using EcoBridge.Domain.Errors;
using EcoBridge.Application.Features.Events;
using Hangfire;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcoBridge.Application.Features.Missions.Commands;

public class ConfirmPickupCommandHandler(
    IEcoBridgeDbContext dbContext,
    IPublisher publisher
) : IRequestHandler<ConfirmPickupCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(ConfirmPickupCommand request, CancellationToken cancellationToken)
    {
        var mission = await dbContext.DeliveryMissions
            .Include(m => m.DonationRequest)
                .ThenInclude(r => r.Donation)
            .FirstOrDefaultAsync(m => m.Id == request.MissionId, cancellationToken);

        if (mission == null)
            return Result<bool>.FailResult(false, AppMessageCodes.MISSION_NOT_FOUND, ErrorType.NotFound);

        if (mission.DonationRequest?.Donation?.DonorId != request.DonorAccountId)
            return Result<bool>.FailResult(false, AppMessageCodes.FORBIDDEN, ErrorType.Forbidden);

        if (mission.MissionStatus != MissionStatus.Accepted)
            return Result<bool>.FailResult(false, AppMessageCodes.MISSION_INVALID_STATUS, ErrorType.Conflict);

        mission.MissionStatus = MissionStatus.InTransit;
        mission.PickupTime = DateTime.UtcNow;

        // Cancel scheduled Hangfire 2-hour timeout job upon successful pickup
        if (!string.IsNullOrEmpty(mission.HangfireJobId))
        {
            BackgroundJob.Delete(mission.HangfireJobId);
            mission.HangfireJobId = null;
        }

        if (mission.DonationRequest?.Donation != null)
        {
            mission.DonationRequest.Donation.Status = DonationStatus.InTransit;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        // Publish event
        await publisher.Publish(new MissionInTransitEvent(
            mission.Id,
            mission.VolunteerId ?? Guid.Empty,
            request.DonorAccountId,
            mission.DonationRequest?.CharityId ?? Guid.Empty
        ), cancellationToken);

        return Result<bool>.SuccessResult(true, AppMessageCodes.MISSION_PICKUP_CONFIRMED);
    }
}
