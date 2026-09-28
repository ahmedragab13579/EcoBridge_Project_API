using EcoBridge.Application.Contracts;
using EcoBridge.Application.Features.Notifications;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EcoBridge.Application.Features.Events.Handlers;

public class TrustScoreIncreasedEventHandler(
    IEcoBridgeDbContext dbContext,
    ICacheService cacheService,
    INotificationService notificationService,
    ILogger<TrustScoreIncreasedEventHandler> logger
) : INotificationHandler<TrustScoreIncreasedEvent>
{
    public async Task Handle(
        TrustScoreIncreasedEvent notification,
        CancellationToken cancellationToken)
    {
        var account = await dbContext.Accounts
            .FirstOrDefaultAsync(a => a.Id == notification.AccountId, cancellationToken);

        if (account == null) return;

        // 1. Audit log
        var creditLog = TrustCreditLog.Create(
            account.Id,
            notification.PointsToAdd,
            notification.ReasonCode,
            notification.MissionId != Guid.Empty ? notification.MissionId : null);

        dbContext.TrustCreditLogs.Add(creditLog);

        // 2. Increase trust score (clamped to MaxTrustScore inside the domain method)
        account.IncreaseTrustScore(notification.PointsToAdd);

        await dbContext.SaveChangesAsync(cancellationToken);

        // 3. Invalidate caches
        await cacheService.RemoveAsync($"user_profile_{account.Id}", cancellationToken);
        await cacheService.RemoveAsync($"user:{account.Id}", cancellationToken);

        // 4. Notify
        await notificationService.CreateNotificationAsync(
            account.Id,
            "Trust Score Restored",
            $"You gained {notification.PointsToAdd} trust points. Reason: {notification.ReasonCode}.",
            NotificationType.PenaltyApplied,
            notification.MissionId,
            cancellationToken);

        logger.LogInformation(
            "Credited {Points} trust points to account {AccountId}. New TrustScore: {TrustScore}",
            notification.PointsToAdd, account.Id, account.TrustScore);
    }
}