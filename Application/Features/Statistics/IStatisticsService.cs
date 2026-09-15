using EcoBridge.Application.DTO;
using EcoBridge.Domain.Results;
using EcoBridge.Domain.Errors;

namespace EcoBridge.Application.Features.Statistics
{
    public interface IStatisticsService
    {
        Task<Result<AdminStatsDTO>> GetAdminStatsAsync(CancellationToken cancellationToken = default);
    }
}
