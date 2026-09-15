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
        logger.LogInformation("Evaluating Volunteer Pickup Timeout SLA (2h) for Mission #{MissionId}, Volunteer #{VolunteerId}",
            missionId, volunteerId);

        var mission = await dbContext.DeliveryMissions
            .Include(m => m.DonationRequest)
                .ThenInclude(r => r.Donation)
            .FirstOrDefaultAsync(m => m.Id == missionId);

        if (mission == null)
        {
            logger.LogWarning("Mission #{MissionId} not found during timeout job execution", missionId);
            return;
        }

        // Only act if mission is still in Accepted state (i.e. not picked up / InTransit / Delivered)
        if (mission.MissionStatus == MissionStatus.Accepted && mission.VolunteerId == volunteerId)
        {
            logger.LogWarning("Volunteer #{VolunteerId} exceeded 2-hour SLA for Mission #{MissionId}. Resetting mission and applying penalty.",
                volunteerId, missionId);

            // 1. Reset mission state
            mission.MissionStatus = MissionStatus.SearchingForVolunteer;
            mission.VolunteerId = null;
            mission.HangfireJobId = null;

            // 2. Restore Volunteer IsAvailable flag
            var volunteer = await dbContext.Volunteers
                .FirstOrDefaultAsync(v => v.AccountId == volunteerId);
            if (volunteer != null)
            {
                volunteer.IsAvailable = true;
            }

            await dbContext.SaveChangesAsync();

            // 3. Apply 15 trust points penalty & audit log (triggers auto-ban if score reaches 0)
            await publisher.Publish(new TrustScoreDeductedEvent(
                volunteerId,
                15,
                "VOLUNTEER_PICKUP_TIMEOUT_2H_SLA",
                missionId
            ));

            // 4. Send timeout notification to the volunteer
            await notificationService.CreateNotificationAsync(
                volunteerId,
                "Mission Timeout & Penalty",
                $"You did not confirm pickup for Mission #{missionId} within the 2-hour SLA window. A 15-point trust score deduction was applied and the mission was reassigned.",
                NotificationType.PenaltyApplied,
                missionId
            );

            // 5. Re-broadcast mission to city group via SignalR
            var cityName = mission.PickupLocation;
            var missionData = new
            {
                missionId = mission.Id,
                donationId = mission.DonationRequest?.DonationId ?? Guid.Empty,
                pickupLocation = mission.PickupLocation,
                dropOffLocation = mission.DropOffLocation,
                createdAt = DateTime.UtcNow,
                reassigned = true
            };

            await hubContext.Clients.All.SendAsync("MissionAvailable", missionData);
            if (!string.IsNullOrWhiteSpace(cityName))
            {
                var normalizedCity = cityName.Trim().ToLowerInvariant();
                await hubContext.Clients.Group($"Group_City_{normalizedCity}").SendAsync("MissionAvailable", missionData);
            }
        }
        else
        {
            logger.LogInformation("Mission #{MissionId} status is {Status}. No timeout action required.",
                missionId, mission.MissionStatus);
        }
    }

    // Recurring scanner: find Accepted missions older than 2 hours and apply timeout logic
    public async Task ExecuteAsync()
    {
        var now = DateTime.UtcNow;
        logger.LogInformation("Running Volunteer Timeout Scanner at {Time}", now);

        var threshold = now.AddHours(-2);

        var missions = await dbContext.DeliveryMissions
            .Include(m => m.DonationRequest)
                .ThenInclude(r => r.Donation)
            .Where(m => m.MissionStatus == MissionStatus.Accepted && m.CreatedAt <= threshold)
            .ToListAsync();

        if (missions.Count == 0)
        {
            logger.LogInformation("No timed-out missions found.");
            return;
        }

        foreach (var mission in missions)
        {
            // Ensure volunteer id exists on mission
            if (!mission.VolunteerId.HasValue || mission.VolunteerId == Guid.Empty)
                continue;

            await ExecuteAsync(mission.Id, mission.VolunteerId.Value);
        }
    }
}
