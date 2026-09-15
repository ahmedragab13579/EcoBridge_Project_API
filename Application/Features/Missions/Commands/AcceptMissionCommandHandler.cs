using EcoBridge.Application.Contracts;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Results;
using EcoBridge.Domain.Errors;
using EcoBridge.Application.DTO;
using EcoBridge.Application.BackgroundJobs;
using EcoBridge.Application.Features.Events;
using Hangfire;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcoBridge.Application.Features.Missions.Commands;

public class AcceptMissionCommandHandler(
    IEcoBridgeDbContext dbContext,
    IPublisher publisher
) : IRequestHandler<AcceptMissionCommand, Result<AcceptMissionResponseDTO>>
{
    public async Task<Result<AcceptMissionResponseDTO>> Handle(AcceptMissionCommand request, CancellationToken cancellationToken)
    {
        var volunteer = await dbContext.Volunteers
            .FirstOrDefaultAsync(v => v.AccountId == request.VolunteerAccountId, cancellationToken);
        if (volunteer == null)
            return Result<AcceptMissionResponseDTO>.FailResult(new AcceptMissionResponseDTO(), AppMessageCodes.VOLUNTEER_NOT_FOUND, ErrorType.NotFound);

        // Atomic update via ExecuteUpdateAsync to prevent race conditions / double-booking without table locks
        var rowsAffected = await dbContext.DeliveryMissions
            .Where(m => m.Id == request.MissionId && m.MissionStatus == MissionStatus.SearchingForVolunteer)
            .ExecuteUpdateAsync(s => s
                .SetProperty(m => m.VolunteerId, volunteer.AccountId)
                .SetProperty(m => m.MissionStatus, MissionStatus.Accepted),
                cancellationToken);

        if (rowsAffected == 0)
        {
            // Mission was either not found or already taken by another volunteer concurrently
            return Result<AcceptMissionResponseDTO>.FailResult(new AcceptMissionResponseDTO(), AppMessageCodes.MISSION_ALREADY_TAKEN, ErrorType.Conflict);
        }

        // Schedule Hangfire 2-Hour Pickup Timeout SLA Job
        var jobId = BackgroundJob.Schedule<VolunteerTimeoutJob>(
            job => job.ExecuteAsync(request.MissionId, volunteer.AccountId),
            TimeSpan.FromHours(2)
        );

        // Update volunteer availability & record Hangfire JobId on the mission
        volunteer.IsAvailable = false;

        var mission = await dbContext.DeliveryMissions
            .FirstOrDefaultAsync(m => m.Id == request.MissionId, cancellationToken);
        if (mission != null)
        {
            mission.HangfireJobId = jobId;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        var pickupLocation = mission?.PickupLocation ?? string.Empty;

        // Publish event for real-time notification
        await publisher.Publish(new MissionAcceptedEvent(
            request.MissionId,
            volunteer.AccountId,
            pickupLocation
        ), cancellationToken);

        var dto = new AcceptMissionResponseDTO
        {
            MissionId = request.MissionId,
            Status = "Accepted",
            PickupLocation = pickupLocation
        };

        return Result<AcceptMissionResponseDTO>.SuccessResult(dto, AppMessageCodes.MISSION_ACCEPTED);
    }
}
