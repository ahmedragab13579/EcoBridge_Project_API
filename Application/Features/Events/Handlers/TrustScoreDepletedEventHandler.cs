using EcoBridge.Application.Contracts;
using EcoBridge.Domain.Enums;
using EcoBridge.Application.Hubs;
using EcoBridge.Application.Features.Notifications;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EcoBridge.Application.Features.Events.Handlers;

public class TrustScoreDepletedEventHandler(
    IEcoBridgeDbContext dbContext,
    IRefreshTokenService refreshTokenService,
    ICacheService cacheService,
    INotificationService notificationService,
    IHubContext<MissionHub> hubContext,
    ILogger<TrustScoreDepletedEventHandler> logger
) : INotificationHandler<TrustScoreDepletedEvent>
{
    public async Task Handle(TrustScoreDepletedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogCritical("ZERO-TOLERANCE PROTOCOL ACTIVATED for Account ID {AccountId}. Reason: {ReasonCode}",
            notification.AccountId, notification.ReasonCode);

        // 1. Deactivate account in Database via rich domain method
        var account = await dbContext.Accounts
            .FirstOrDefaultAsync(a => a.Id == notification.AccountId, cancellationToken);

        if (account != null)
        {
            account.Deactivate();
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        // 2. Revoke all active refresh tokens in Redis
        await refreshTokenService.RevokeAllUserTokensAsync(notification.AccountId, cancellationToken);

        // 3. Invalidate Redis profile caches
        await cacheService.RemoveAsync($"user_profile_{notification.AccountId}", cancellationToken);
        await cacheService.RemoveAsync($"user:{notification.AccountId}", cancellationToken);

        // 4. Send Account Suspension In-App Notification
        await notificationService.CreateNotificationAsync(
            notification.AccountId,
            "Account Suspended",
            "Your account has been permanently suspended due to your Trust Score reaching 0 (Zero-Tolerance Protocol violation).",
            NotificationType.PenaltyApplied,
            notification.MissionId,
            cancellationToken
        );

        // 5. Broadcast real-time AccountSuspended SignalR event to user connection
        await hubContext.Clients.Group($"User_{notification.AccountId}")
            .SendAsync("AccountSuspended", new
            {
                accountId = notification.AccountId,
                reason = notification.ReasonCode,
                message = "Account suspended due to depleted trust score."
            }, cancellationToken);
    }
}
