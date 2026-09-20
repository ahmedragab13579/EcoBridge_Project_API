using EcoBridge.Application.Contracts;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Models;
using EcoBridge.Application.Features.Notifications;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EcoBridge.Application.Features.Events.Handlers;

public class TrustScoreDeductedEventHandler(
    IEcoBridgeDbContext dbContext,
    ICacheService cacheService,
    INotificationService notificationService,
    IPublisher publisher,
    ILogger<TrustScoreDeductedEventHandler> logger
) : INotificationHandler<TrustScoreDeductedEvent>
{
    public async Task Handle(TrustScoreDeductedEvent notification, CancellationToken cancellationToken)
    {
        var account = await dbContext.Accounts.FirstOrDefaultAsync(a => a.Id == notification.AccountId, cancellationToken);
        if (account == null) return;

        // 1. Audit log in PenaltyLogs table
        var penaltyLog = PenaltyLog.Create(account.Id, notification.PointsToDeduct, notification.ReasonCode, notification.MissionId != Guid.Empty ? notification.MissionId : null);
       
        dbContext.PenaltyLogs.Add(penaltyLog);

        // 2. Deduct trust score via rich domain method
        account.DecreaseTrustScore(notification.PointsToDeduct);
        await dbContext.SaveChangesAsync(cancellationToken);

        // 3. Invalidate Redis profile caches
        await cacheService.RemoveAsync($"user_profile_{account.Id}", cancellationToken);
        await cacheService.RemoveAsync($"user:{account.Id}", cancellationToken);

        // 4. Send In-App Notification
        await notificationService.CreateNotificationAsync(
            account.Id,
            "Trust Score Deducted",
            $"A penalty of {notification.PointsToDeduct} trust points was applied to your account due to review violation ({notification.ReasonCode}).",
            NotificationType.PenaltyApplied,
            notification.MissionId,
            cancellationToken
        );

        logger.LogWarning("Deducted {Points} trust points from account {AccountId}. New TrustScore: {TrustScore}",
            notification.PointsToDeduct, account.Id, account.TrustScore);

        // 5. Zero-Tolerance Trigger: If TrustScore depleted to 0, trigger Auto-Ban Protocol
        if (account.TrustScore <= 0)
        {
            await publisher.Publish(new TrustScoreDepletedEvent(
                account.Id,
                notification.ReasonCode,
                notification.MissionId != Guid.Empty ? notification.MissionId : null
            ), cancellationToken);
        }
    }
}
