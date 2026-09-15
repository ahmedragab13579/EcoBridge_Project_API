using MediatR;

namespace EcoBridge.Application.Features.Events;

public record DeliveryCompletedEvent(
    Guid MissionId,
    Guid VolunteerId,
    Guid CharityId,
    Guid DonorId
) : INotification;
