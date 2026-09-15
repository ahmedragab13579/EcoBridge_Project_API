using MediatR;

namespace EcoBridge.Application.Features.Events;

public record MissionAcceptedEvent(
    Guid MissionId,
    Guid VolunteerId,
    string PickupLocation
) : INotification;
