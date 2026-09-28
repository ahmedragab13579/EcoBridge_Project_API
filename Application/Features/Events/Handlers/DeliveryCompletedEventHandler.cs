using EcoBridge.Application.Contracts;
using EcoBridge.Application.Features.Notifications;
using EcoBridge.Application.Hubs;
using EcoBridge.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace EcoBridge.Application.Features.Events.Handlers;

public class DeliveryCompletedEventHandler(
    IEcoBridgeDbContext dbContext,
    IHubContext<MissionHub> hubContext,
    INotificationService notificationService,
    ICacheService cacheService
) : INotificationHandler<DeliveryCompletedEvent>
{
    private const int RewardPoints = 50;

    public async Task Handle(DeliveryCompletedEvent notification, CancellationToken cancellationToken)
    {
        // 1. Invalidate cached profiles so dashboards reflect new points/trust
        await cacheService.RemoveAsync($"user_profile_{notification.VolunteerId}", cancellationToken);

        if (notification.CharityId != Guid.Empty)
            await cacheService.RemoveAsync($"user_profile_{notification.CharityId}", cancellationToken);

        // 2. Notify volunteer of points earned
        await notificationService.CreateNotificationAsync(
            notification.VolunteerId,
            "Delivery Completed & Points Earned!",
            $"Congratulations! Mission #{notification.MissionId} has been successfully verified. You earned {RewardPoints} wallet points.",
            NotificationType.DeliveryCompleted,
            notification.MissionId,
            cancellationToken);

        // 3. Notify charity of successful delivery
        if (notification.CharityId != Guid.Empty)
        {
            await notificationService.CreateNotificationAsync(
                notification.CharityId,
                "Delivery Verified",
                $"Donation for Mission #{notification.MissionId} was confirmed and successfully delivered.",
                NotificationType.DeliveryCompleted,
                notification.MissionId,
                cancellationToken);
        }

        // 4. Notify the donor (look up via donation) — optional; remove if not needed
        var donorId = await dbContext.Donations
            .Where(d => d.Id == notification.DonorId)
            .Select(d => (Guid?)d.DonorId)
            .FirstOrDefaultAsync(cancellationToken);

        if (donorId is Guid donor && donor != Guid.Empty)
        {
            await notificationService.CreateNotificationAsync(
                donor,
                "Your Donation Was Delivered",
                $"Good news! Your donation for Mission #{notification.MissionId} has been delivered to the charity.",
                NotificationType.DeliveryCompleted,
                notification.MissionId,
                cancellationToken);

            await hubContext.Clients.Group($"User_{donor}")
                .SendAsync("DeliveryCompleted",
                    new { missionId = notification.MissionId },
                    cancellationToken);
        }

        // 5. Push real-time completion to volunteer & charity
        await hubContext.Clients.Group($"User_{notification.VolunteerId}")
            .SendAsync("DeliveryCompleted",
                new { missionId = notification.MissionId, pointsAwarded = RewardPoints },
                cancellationToken);

        if (notification.CharityId != Guid.Empty)
        {
            await hubContext.Clients.Group($"User_{notification.CharityId}")
                .SendAsync("DeliveryCompleted",
                    new { missionId = notification.MissionId },
                    cancellationToken);
        }
    }
}