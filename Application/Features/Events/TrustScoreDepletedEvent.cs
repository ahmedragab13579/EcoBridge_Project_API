using MediatR;

namespace EcoBridge.Application.Features.Events;

public record TrustScoreDepletedEvent(
    Guid AccountId,
    string ReasonCode,
    Guid? MissionId = null
) : INotification;
