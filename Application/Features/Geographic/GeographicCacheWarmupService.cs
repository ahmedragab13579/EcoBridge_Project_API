using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EcoBridge.Application.Features.Geographic;

public class GeographicCacheWarmupService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<GeographicCacheWarmupService> _logger;

    public GeographicCacheWarmupService(
        IServiceProvider serviceProvider,
        ILogger<GeographicCacheWarmupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("GeographicCacheWarmupService starting...");

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var geoService = scope.ServiceProvider.GetRequiredService<IGeographicService>();
            await geoService.WarmupCacheAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while warming up the geographic cache.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
