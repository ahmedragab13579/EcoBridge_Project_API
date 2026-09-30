using EcoBridge.Application.Contracts;
using EcoBridge.Application.DTO;
using EcoBridge.Application.Features.Events;
using EcoBridge.Application.Result;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Exceptions;
using EcoBridge.Domain.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcoBridge.Application.Features.Missions.Commands;

public class CompleteMissionCommandHandler(
    IEcoBridgeDbContext dbContext,
    IOutboxService outboxService
) : IRequestHandler<CompleteMissionCommand, ApiResult<CompleteMissionResponseDTO>>
{
    private const int RewardPoints = 50;

    public async Task<ApiResult<CompleteMissionResponseDTO>> Handle(
        CompleteMissionCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Load mission with the related donation so we can update its status too
        var mission = await dbContext.DeliveryMissions
            .Include(m => m.Donation)
            .FirstOrDefaultAsync(m => m.Id == request.MissionId, cancellationToken);

        if (mission == null)
            throw new DomainException.NotFoundException(AppMessageCodes.MISSION_NOT_FOUND);
        if (mission.Status == MissionStatus.Cancelled)
            throw new DomainException.ConflictException(AppMessageCodes.MISSION_ALREADY_CANCELLED);
        if (mission.Status == MissionStatus.Failed)
            throw new DomainException.ConflictException(AppMessageCodes.MISSION_ALREADY_FAILED);
        if (mission.Status == MissionStatus.Delivered)
            throw new DomainException.ConflictException(AppMessageCodes.MISSION_ALREADY_DELIVERED);

        // 2. Only the assigned volunteer may complete this mission
        if (mission.VolunteerId != request.VolunteerId)
            throw new DomainException.ForbiddenException(AppMessageCodes.FORBIDDEN);

        // 3. State guards
        if (mission.DeliveryTime is not null)
            throw new DomainException.ConflictException(AppMessageCodes.MISSION_ALREADY_DELIVERED);

        if (mission.CancelledAtUtc is not null)
            throw new DomainException.ConflictException(AppMessageCodes.MISSION_ALREADY_CANCELLED);

        if (mission.PickupTime is null)
            throw new DomainException.ConflictException(AppMessageCodes.MISSION_NOT_IN_TRANSIT);

        // 4. Verify OTP. On failure, the domain increments OtpAttemptCount,
        //    so we MUST save before letting the exception bubble up.
        try
        {
            mission.VerifyAndConsumeOtp(request.Otp);
        }
        catch (DomainException)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            throw;
        }

        // 5. Mark delivery complete (idempotent; also clears OTP)
        mission.RecordDelivery();
        mission.Donation.Complete();

        // 6. Free the volunteer and award points
        var volunteer = await dbContext.Accounts
            .FirstOrDefaultAsync(a => a.Id == request.VolunteerId, cancellationToken);

        if (volunteer is null)
            throw new DomainException.NotFoundException(AppMessageCodes.VOLUNTEER_NOT_FOUND);

        volunteer.SetAvailability(true);
        volunteer.AddPoints(RewardPoints);

        var pointTransaction = PointTransaction.Create(
            volunteer.Id,
            RewardPoints,
            TransactionType.MissionCompleted,
            mission.Id);

        dbContext.PointTransactions.Add(pointTransaction);

        // 7. Enqueue DeliveryCompletedEvent to durable outbox before committing
        outboxService.Enqueue(new DeliveryCompletedEvent(
            mission.Id,
            volunteer.Id,
            mission.Donation.CharityId ?? Guid.Empty,
            mission.Donation.DonorId));

        // 8. Persist everything in a single atomic save
        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResult<CompleteMissionResponseDTO>.Ok(
            new CompleteMissionResponseDTO
            {
                MissionId = mission.Id,
                PointsAwarded = RewardPoints
            },
            AppMessageCodes.MISSION_COMPLETED);
    }
}