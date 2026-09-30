using EcoBridge.Application.Contracts;
using EcoBridge.Application.Features.Events;
using EcoBridge.Application.Features.Missions.Commands;
using EcoBridge.Application.Result;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcoBridge.Application.Features.Missions.Handlers;

public class ConfirmPickupCommandHandler(
    IEcoBridgeDbContext dbContext,
    IOutboxService outboxService
) : IRequestHandler<ConfirmPickupCommand, ApiResult<bool>>
{
    public async Task<ApiResult<bool>> Handle(
        ConfirmPickupCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Load mission with donation
        var mission = await dbContext.DeliveryMissions
            .Include(m => m.Donation)
            .FirstOrDefaultAsync(m => m.Id == request.MissionId, cancellationToken);

        if (mission == null)
            throw new DomainException.NotFoundException(AppMessageCodes.MISSION_NOT_FOUND);

        // 2. Only the assigned volunteer may confirm pickup
        if (mission.VolunteerId != request.VolunteerId)
            throw new DomainException.ForbiddenException(AppMessageCodes.FORBIDDEN);

        // 3. State guards
        if (mission.DeliveryTime is not null)
            throw new DomainException.ConflictException(AppMessageCodes.MISSION_ALREADY_DELIVERED);

        if (mission.CancelledAtUtc is not null)
            throw new DomainException.ConflictException(AppMessageCodes.MISSION_ALREADY_CANCELLED);

        // 4. Idempotency: if already picked up, return success without duplicate side effects
        if (mission.PickupTime is not null)
            return ApiResult<bool>.Ok(true, AppMessageCodes.MISSION_IN_TRANSIT);

        // 5. Confirm pickup on the mission and move the donation to InTransit
        mission.ConfirmPickup();

        // 6. Durable outbox event persistence within the same database transaction
        outboxService.Enqueue(new MissionInTransitEvent(
            mission.Id,
            mission.DonationId,
            mission.VolunteerId,
            mission.Donation.CharityId ?? Guid.Empty
        ));

        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResult<bool>.Ok(true, AppMessageCodes.MISSION_IN_TRANSIT);
    }
}