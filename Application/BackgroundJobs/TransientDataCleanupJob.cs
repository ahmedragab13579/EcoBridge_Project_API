using EcoBridge.Application.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EcoBridge.Application.BackgroundJobs;

public class TransientDataCleanupJob(
    IServiceScopeFactory scopeFactory,
    ILogger<TransientDataCleanupJob> logger
)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var notificationCutoff = now.AddDays(-30);
        var outboxCutoff = now.AddDays(-7);
        logger.LogInformation("Starting Transient Data Retention Cleanup at {Time}", now);

        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IEcoBridgeDbContext>();

        // 1. Purge Notifications older than 30 days
        int deletedNotifications;
        try
        {
            deletedNotifications = await dbContext.Notifications
                .Where(n => n.CreatedAtUtc < notificationCutoff)
                .ExecuteDeleteAsync(cancellationToken);
        }
        catch (InvalidOperationException)
        {
            var staleNotifications = await dbContext.Notifications
                .Where(n => n.CreatedAtUtc < notificationCutoff)
                .ToListAsync(cancellationToken);

            dbContext.Notifications.RemoveRange(staleNotifications);
            deletedNotifications = await dbContext.SaveChangesAsync(cancellationToken);
        }

        logger.LogInformation("Purged {Count} notifications older than 30 days (prior to {Cutoff}).", deletedNotifications, notificationCutoff);

        // 2. Purge Processed Outbox Messages older than 7 days
        int deletedOutbox;
        try
        {
            deletedOutbox = await dbContext.OutboxMessages
                .Where(m => m.ProcessedOnUtc != null && m.ProcessedOnUtc < outboxCutoff)
                .ExecuteDeleteAsync(cancellationToken);
        }
        catch (InvalidOperationException)
        {
            var staleOutbox = await dbContext.OutboxMessages
                .Where(m => m.ProcessedOnUtc != null && m.ProcessedOnUtc < outboxCutoff)
                .ToListAsync(cancellationToken);

            dbContext.OutboxMessages.RemoveRange(staleOutbox);
            deletedOutbox = await dbContext.SaveChangesAsync(cancellationToken);
        }

        logger.LogInformation("Purged {Count} processed outbox messages older than 7 days.", deletedOutbox);

        // 3. Purge Expired or Revoked Refresh Tokens (EF fallback)
        int deletedTokens;
        try
        {
            deletedTokens = await dbContext.RefreshTokens
                .Where(t => t.ExpiresAtUtc < now || t.RevokedAtUtc != null)
                .ExecuteDeleteAsync(cancellationToken);
        }
        catch (InvalidOperationException)
        {
            var staleTokens = await dbContext.RefreshTokens
                .Where(t => t.ExpiresAtUtc < now || t.RevokedAtUtc != null)
                .ToListAsync(cancellationToken);

            dbContext.RefreshTokens.RemoveRange(staleTokens);
            deletedTokens = await dbContext.SaveChangesAsync(cancellationToken);
        }

        logger.LogInformation("Purged {Count} expired or revoked refresh tokens.", deletedTokens);
        logger.LogInformation("Completed Transient Data Retention Cleanup.");
    }
}