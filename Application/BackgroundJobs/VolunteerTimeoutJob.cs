using EcoBridge.Application.Contracts;
using EcoBridge.Domain.Enums;
using EcoBridge.Application.Hubs;
using EcoBridge.Application.Features.Events;
using EcoBridge.Application.Features.Notifications;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EcoBridge.Application.BackgroundJobs;

public class VolunteerTimeoutJob(
    IServiceScopeFactory scopeFactory,
    ILogger<VolunteerTimeoutJob> logger
)
{
    public async Task ExecuteAsync(Guid missionId, Guid volunteerId, CancellationToken cancellationToken = default)
    {
        // 1. Isolated scope per execution unit to guarantee thread-safe DbContext
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IEcoBridgeDbContext>();
        var publisher = scope.ServiceProvider.GetRequiredService<IPublisher>();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
        var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<MissionHub>>();

        logger.LogInformation(
            "Evaluating Volunteer Pickup Timeout SLA (2h) for Mission #{MissionId}, Volunteer #{VolunteerId}",
            missionId,
            volunteerId);

        var mission = await dbContext.DeliveryMissions
            .Include(m => m.Donation)
            .FirstOrDefaultAsync(m => m.Id == missionId, cancellationToken);

        if (mission is null)
        {
            logger.LogWarning("Mission #{MissionId} not found during timeout job execution", missionId);
            return;
        }

        var donation = mission.Donation;
        if (donation is null || donation.Status != DonationStatus.VolunteerAssigned || donation.VolunteerId != volunteerId)
        {
            logger.LogInformation(
                "Mission #{MissionId} is no longer assigned to volunteer #{VolunteerId}. No timeout action required.",
                missionId,
                volunteerId);
            return;
        }

        logger.LogWarning(
            "Volunteer #{VolunteerId} exceeded the 2-hour SLA for Mission #{MissionId}. Resetting assignment and applying penalty.",
            volunteerId,
            missionId);

        donation.ReturnToVolunteerSearch();

        var volunteer = await dbContext.Accounts
            .FirstOrDefaultAsync(v => v.Id == volunteerId, cancellationToken);

        volunteer?.SetAvailability(true);

        // 2. Safe concurrency handling against race conditions
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            logger.LogWarning(ex, "Concurrency conflict for Mission #{MissionId}. State was updated concurrently by user.", missionId);
            return; // Skip events/notifications if state changed concurrently
        }

        // 3. Dispatch events & notifications only after a successful DB save
        await publisher.Publish(new TrustScoreDeductedEvent(
            volunteerId,
            15,
            "VOLUNTEER_PICKUP_TIMEOUT_2H_SLA",
            missionId
        ), cancellationToken);

        await notificationService.CreateNotificationAsync(
            volunteerId,
            "Mission Timeout & Penalty",
            $"You did not confirm pickup for Mission #{missionId} within the 2-hour SLA window. A 15-point trust score deduction was applied and the mission was reopened for reassignment.",
            NotificationType.PenaltyApplied,
            missionId,
            cancellationToken
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

        await hubContext.Clients.All.SendAsync("MissionAvailable", missionData, cancellationToken);

        if (!string.IsNullOrWhiteSpace(donation.PickupLocation))
        {
            var normalizedCity = donation.PickupLocation.Trim().ToLowerInvariant();
            await hubContext.Clients.Group($"Group_City_{normalizedCity}").SendAsync("MissionAvailable", missionData, cancellationToken);
        }
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        logger.LogInformation("Running Volunteer Timeout Scanner at {Time}", now);

        List<(Guid MissionId, Guid VolunteerId)> targetMissions;

        // Step 1: Read-only scan in a quick isolated scope
        using (var scope = scopeFactory.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<IEcoBridgeDbContext>();
            var threshold = now.AddHours(-2);

            var items = await dbContext.DeliveryMissions
                .AsNoTracking()
                .Include(m => m.Donation)
                .Where(m =>
                    m.CreatedAt <= threshold &&
                    m.PickupTime == null &&
                    m.CancelledAtUtc == null &&
                    m.Donation.Status == DonationStatus.VolunteerAssigned &&
                    m.VolunteerId != Guid.Empty)
                .Select(m => new { m.Id, VolunteerId = m.VolunteerId })
                .ToListAsync(cancellationToken);

            targetMissions = items.Select(x => (x.Id, x.VolunteerId)).ToList();
        }

        if (targetMissions.Count == 0)
        {
            logger.LogInformation("No timed-out missions found.");
            return;
        }

        // Step 2: Process each timed-out mission inside its own isolated unit of work
        foreach (var (missionId, volunteerId) in targetMissions)
        {
            try
            {
                await ExecuteAsync(missionId, volunteerId, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to process timeout SLA for Mission #{MissionId}", missionId);
            }
        }
    }
}