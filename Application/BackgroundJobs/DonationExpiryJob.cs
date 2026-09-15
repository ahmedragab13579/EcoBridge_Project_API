using EcoBridge.Application.Contracts;
using EcoBridge.Domain.Enums;
using EcoBridge.Application.Features.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EcoBridge.Application.BackgroundJobs;

public class DonationExpiryJob(
    IEcoBridgeDbContext dbContext,
    INotificationService notificationService,
    ILogger<DonationExpiryJob> logger
)
{
    public async Task ExecuteAsync()
    {
        var now = DateTime.UtcNow;
        logger.LogInformation("Running Hourly Donation Expiry Monitor at {Time}", now);

        var expiredDonations = await dbContext.Donations
            .Where(d => d.Status == DonationStatus.Available && d.ExpiryDate < now && !d.IsDeleted)
            .ToListAsync();

        if (expiredDonations.Count == 0)
        {
            logger.LogInformation("No expired donations found.");
            return;
        }

        logger.LogInformation("Found {Count} expired donation(s). Updating status and notifying donors.", expiredDonations.Count);

        foreach (var donation in expiredDonations)
        {
            donation.Status = DonationStatus.Expired;

            // Notify Donor that their donation has expired
            await notificationService.CreateNotificationAsync(
                donation.DonorId,
                "Donation Expired",
                $"Your donation '{donation.Title ?? donation.FoodType}' has reached its expiry date and is no longer available.",
                NotificationType.DonationStatusChanged,
                donation.Id
            );
        }

        await dbContext.SaveChangesAsync();
        logger.LogInformation("Successfully expired {Count} donation(s).", expiredDonations.Count);
    }
}
