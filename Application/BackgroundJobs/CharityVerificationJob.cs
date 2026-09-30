using EcoBridge.Application.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EcoBridge.Application.BackgroundJobs;

public class CharityVerificationJob(
    IServiceScopeFactory scopeFactory,
    ILogger<CharityVerificationJob> logger
)
{
    // Periodic job placeholder: currently logs and can be extended to process verification requests
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        logger.LogInformation("Running Charity Verification Scanner at {Time}", now);

        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IEcoBridgeDbContext>();

        // For now, just log the count of charity records. Future: process pending verification queue.
        var count = await dbContext.Accounts
            .CountAsync(a => a.Role == EcoBridge.Domain.Enums.UserRole.Charity, cancellationToken);

        logger.LogInformation("Charity records count: {Count}", count);
    }
}