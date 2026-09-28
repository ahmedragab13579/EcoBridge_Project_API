using MediatR;

namespace EcoBridge.Application.Features.Events;

public record TrustScoreIncreasedEvent(
    Guid AccountId,
    int PointsToAdd,
    string ReasonCode,
    Guid MissionId
) : INotification;