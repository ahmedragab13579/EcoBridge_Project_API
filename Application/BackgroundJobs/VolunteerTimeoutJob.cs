using EcoBridge.Application.Contracts;
using EcoBridge.Domain.Enums;
using EcoBridge.Application.Hubs;
using EcoBridge.Application.Features.Events;
using EcoBridge.Application.Features.Notifications;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EcoBridge.Application.BackgroundJobs;

public class VolunteerTimeoutJob(
    IEcoBridgeDbContext dbContext,
    IPublisher publisher,
    INotificationService notificationService,
    IHubContext<MissionHub> hubContext,
    ILogger<VolunteerTimeoutJob> logger
)
{
    public async Task ExecuteAsync(Guid missionId, Guid volunteerId)
    {
        logger.LogInformation(
            "Evaluating Volunteer Pickup Timeout SLA (2h) for Mission #{MissionId}, Volunteer #{VolunteerId}",
            missionId,
            volunteerId);

        var mission = await dbContext.DeliveryMissions
            .Include(m => m.Donation)
            .FirstOrDefaultAsync(m => m.Id == missionId);

        if (mission == null)
        {
            logger.LogWarning("Mission #{MissionId} not found during timeout job execution", missionId);
            return;
        }

        var donation = mission.Donation;
        if (donation.Status != DonationStatus.VolunteerAssigned || donation.VolunteerId != volunteerId)
        {
            logger.LogInformation(
                "Mission #{MissionId} is no longer assigned to volunteer #{VolunteerId}. No timeout action required.",
                missionId,
                volunteerId);
            return;
        }

        logger.LogWarning(
            "Volunteer #{VolunteerId} exceeded the 2-hour SLA for Mission #{MissionId}. Resetting the assignment and applying a penalty.",
            volunteerId,
            missionId);

        donation.TimeoutVolunteerAssignment(volunteerId, "Volunteer pickup timeout");

        var volunteer = await dbContext.Accounts
            .FirstOrDefaultAsync(v => v.Id == volunteerId);
        if (volunteer != null)
        {
            volunteer.SetAvailability(true);
        }

        await dbContext.SaveChangesAsync();

        await publisher.Publish(new TrustScoreDeductedEvent(
            volunteerId,
            15,
            "VOLUNTEER_PICKUP_TIMEOUT_2H_SLA",
            missionId
        ));

        await notificationService.CreateNotificationAsync(
            volunteerId,
            "Mission Timeout & Penalty",
            $"You did not confirm pickup for Mission #{missionId} within the 2-hour SLA window. A 15-point trust score deduction was applied and the mission was reopened for reassignment.",
            NotificationType.PenaltyApplied,
            missionId
        );

        var missionData = new
        {
            missionId = mission.Id,
            donationId = donation.Id,
            pickupLocation = donation.PickupLocation,
            dropOffLocation = mission.DropOffLocation,
            createdAt = DateTime.UtcNow,
            reassigned = true
        };

        await hubContext.Clients.All.SendAsync("MissionAvailable", missionData);

        var cityName = donation.PickupLocation;
        if (!string.IsNullOrWhiteSpace(cityName))
        {
            var normalizedCity = cityName.Trim().ToLowerInvariant();
            await hubContext.Clients.Group($"Group_City_{normalizedCity}").SendAsync("MissionAvailable", missionData);
        }
    }

    public async Task ExecuteAsync()
    {
        var now = DateTime.UtcNow;
        logger.LogInformation("Running Volunteer Timeout Scanner at {Time}", now);

        var threshold = now.AddHours(-2);

        var missions = await dbContext.DeliveryMissions
            .Include(m => m.Donation)
            .Where(m =>
                m.CreatedAt <= threshold &&
                m.PickupTime == null &&
                m.CancelledAtUtc == null &&
                m.Donation.Status == DonationStatus.VolunteerAssigned)
            .ToListAsync();

        if (missions.Count == 0)
        {
            logger.LogInformation("No timed-out missions found.");
            return;
        }

        foreach (var mission in missions)
        {
            if (mission.VolunteerId == Guid.Empty)
                continue;

            await ExecuteAsync(mission.Id, mission.VolunteerId);
        }
    }
}
