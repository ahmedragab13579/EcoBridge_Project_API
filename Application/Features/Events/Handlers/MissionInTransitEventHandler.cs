using EcoBridge.Domain.Enums;
using EcoBridge.Application.Hubs;
using EcoBridge.Application.Features.Notifications;
using MediatR;
using Microsoft.AspNetCore.SignalR;

namespace EcoBridge.Application.Features.Events.Handlers;

public class MissionInTransitEventHandler(
    IHubContext<MissionHub> hubContext,
    INotificationService notificationService
) : INotificationHandler<MissionInTransitEvent>
{
    public async Task Handle(MissionInTransitEvent notification, CancellationToken cancellationToken)
    {
        // Notify Charity that items are on the way
        if (notification.CharityId != Guid.Empty)
        {
            await notificationService.CreateNotificationAsync(
                notification.CharityId,
                "Donation In Transit",
                $"Volunteer has picked up Donation #{notification.MissionId} and is heading to your dropoff location.",
                NotificationType.MissionAssigned,
                notification.MissionId,
                cancellationToken);
        }

        // Push SignalR message to Charity group
        await hubContext.Clients.Group($"User_{notification.CharityId}")
            .SendAsync("MissionInTransit", new { missionId = notification.MissionId }, cancellationToken);
    }
}
