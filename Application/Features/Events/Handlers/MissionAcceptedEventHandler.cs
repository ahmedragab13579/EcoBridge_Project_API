using EcoBridge.Domain.Enums;
using EcoBridge.Application.Hubs;
using EcoBridge.Application.Features.Notifications;
using MediatR;
using Microsoft.AspNetCore.SignalR;

namespace EcoBridge.Application.Features.Events.Handlers;

public class MissionAcceptedEventHandler(
    IHubContext<MissionHub> hubContext,
    INotificationService notificationService
) : INotificationHandler<MissionAcceptedEvent>
{
    public async Task Handle(MissionAcceptedEvent notification, CancellationToken cancellationToken)
    {
        // Broadcast MissionTaken immediately so all other volunteers' UI removes this mission card
        await hubContext.Clients.All.SendAsync("MissionTaken", new
        {
            missionId = notification.MissionId,
            volunteerId = notification.VolunteerId
        }, cancellationToken);

        // Send confirmation notification to volunteer
        await notificationService.CreateNotificationAsync(
            notification.VolunteerId,
            "Mission Assigned",
            $"You have successfully accepted Mission #{notification.MissionId}. Please proceed to pickup location.",
            NotificationType.MissionAssigned,
            notification.MissionId,
            cancellationToken);
    }
}
