using EcoBridge.Application.Contracts;
using EcoBridge.Domain.Enums;
using EcoBridge.Application.Features.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EcoBridge.Application.BackgroundJobs;

public class DonationExpiryJob(
    IServiceScopeFactory scopeFactory,
    ILogger<DonationExpiryJob> logger
)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        logger.LogInformation("Running Hourly Donation Expiry Monitor at {Time}", now);

        List<Guid> expiredDonationIds;

        // Step 1: Fetch expired donation IDs in a lightweight read-only scope
        using (var scope = scopeFactory.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<IEcoBridgeDbContext>();

            expiredDonationIds = await dbContext.Donations
                .AsNoTracking()
                .Where(d => d.Status == DonationStatus.Available && d.ExpiryDate < now && !d.IsDeleted)
                .Select(d => d.Id)
                .ToListAsync(cancellationToken);
        }

        if (expiredDonationIds.Count == 0)
        {
            logger.LogInformation("No expired donations found.");
            return;
        }

        logger.LogInformation("Found {Count} expired donation(s). Processing status updates and notifications.", expiredDonationIds.Count);

        // Step 2: Process each donation in its own isolated scope
        foreach (var donationId in expiredDonationIds)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<IEcoBridgeDbContext>();
                var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                var donation = await dbContext.Donations
                    .FirstOrDefaultAsync(d => d.Id == donationId, cancellationToken);

                if (donation is null || donation.Status != DonationStatus.Available || donation.IsDeleted)
                    continue;

                donation.Expire();

                try
                {
                    await dbContext.SaveChangesAsync(cancellationToken);
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    logger.LogWarning(ex, "Concurrency conflict while expiring Donation #{DonationId}. State updated concurrently.", donationId);
                    continue;
                }

                // Send Notification after successful state persist
                await notificationService.CreateNotificationAsync(
                    donation.DonorId,
                    "Donation Expired",
                    $"Your donation '{donation.Title ?? donation.FoodType}' has reached its expiry date and is no longer available.",
                    NotificationType.DonationStatusChanged,
                    donation.Id,
                    cancellationToken
                );
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to process expiration for Donation #{DonationId}", donationId);
            }
        }

        logger.LogInformation("Completed processing expired donations execution cycle.");
    }
}