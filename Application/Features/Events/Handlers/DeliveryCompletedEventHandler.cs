using EcoBridge.Application.Contracts;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Models;
using EcoBridge.Application.Hubs;
using EcoBridge.Application.Features.Notifications;
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
        // 1. Restore volunteer IsAvailable flag & award wallet points
        var volunteerAccount = await dbContext.Accounts
            .FirstOrDefaultAsync(a => a.Id == notification.VolunteerId, cancellationToken);

        if (volunteerAccount != null)
        {
            volunteerAccount.SetAvailability(true);
            volunteerAccount.AddPoints(RewardPoints);

            var pointTransaction = PointTransaction.Create(
                volunteerAccount.Id,
                RewardPoints,
                TransactionType.MissionCompleted,
                notification.MissionId
            );

            dbContext.PointTransactions.Add(pointTransaction);
            await dbContext.SaveChangesAsync(cancellationToken);

            // Invalidate cached user profile
            await cacheService.RemoveAsync($"user_profile_{volunteerAccount.Id}", cancellationToken);

            // Notify Volunteer of points earned
            await notificationService.CreateNotificationAsync(
                volunteerAccount.Id,
                "Delivery Completed & Points Earned!",
                $"Congratulations! Mission #{notification.MissionId} has been successfully verified. You earned {RewardPoints} wallet points.",
                NotificationType.DeliveryCompleted,
                notification.MissionId,
                cancellationToken);
        }
        else
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        // 2. Notify Charity of successful delivery
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

        // 3. Push real-time SignalR completion message to volunteer & charity
        await hubContext.Clients.Group($"User_{notification.VolunteerId}")
            .SendAsync("DeliveryCompleted", new { missionId = notification.MissionId, pointsAwarded = RewardPoints }, cancellationToken);

        await hubContext.Clients.Group($"User_{notification.CharityId}")
            .SendAsync("DeliveryCompleted", new { missionId = notification.MissionId }, cancellationToken);
    }
}
