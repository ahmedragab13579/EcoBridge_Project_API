using MediatR;

namespace EcoBridge.Application.Features.Events;

public record MissionInTransitEvent(
    Guid MissionId,
    Guid VolunteerId,
    Guid DonorId,
    Guid CharityId
) : INotification;
