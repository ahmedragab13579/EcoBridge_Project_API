using EcoBridge.Application.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EcoBridge.Application.BackgroundJobs;

public class TransientDataCleanupJob(
    IEcoBridgeDbContext dbContext,
    ILogger<TransientDataCleanupJob> logger
)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var notificationCutoff = now.AddDays(-30);
        logger.LogInformation("Starting Transient Data Retention Cleanup at {Time}", now);

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
            // Fallback for providers that do not support bulk ExecuteDeleteAsync (e.g. in-memory test providers)
            var staleNotifications = await dbContext.Notifications
                .Where(n => n.CreatedAtUtc < notificationCutoff)
                .ToListAsync(cancellationToken);
            dbContext.Notifications.RemoveRange(staleNotifications);
            deletedNotifications = await dbContext.SaveChangesAsync(cancellationToken);
        }

        logger.LogInformation("Purged {Count} notifications older than 30 days (prior to {Cutoff}).", deletedNotifications, notificationCutoff);

        // 2. Purge Expired or Revoked Refresh Tokens
        int deletedTokens;
        try
        {
            deletedTokens = await dbContext.RefreshTokens
                .Where(t => t.ExpiresAtUtc < now || t.RevokedAtUtc != null)
                .ExecuteDeleteAsync(cancellationToken);
        }
        catch (InvalidOperationException)
        {
            // Fallback for providers that do not support bulk ExecuteDeleteAsync (e.g. in-memory test providers)
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
