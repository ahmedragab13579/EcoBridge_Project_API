using MediatR;

namespace EcoBridge.Application.Features.Events;

public record TrustScoreDeductedEvent(
    Guid AccountId,
    int PointsToDeduct,
    string ReasonCode,
    Guid MissionId
) : INotification;
