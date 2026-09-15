using EcoBridge.Application.Contracts;
using EcoBridge.Application.Features.Notifications;
using Microsoft.Extensions.Logging;

namespace EcoBridge.Application.BackgroundJobs;

public class CharityVerificationJob(
    IEcoBridgeDbContext dbContext,
    INotificationService notificationService,
    ILogger<CharityVerificationJob> logger
)
{
    // Periodic job placeholder: currently logs and can be extended to process verification requests
    public async Task ExecuteAsync()
    {
        logger.LogInformation("Running Charity Verification Scanner at {Time}", DateTime.UtcNow);

        // For now, just log the count of charity records. Future: process pending verification queue.
        var count =  dbContext.Charities.Count();
        logger.LogInformation("Charity records count: {Count}", count);
    }
}
