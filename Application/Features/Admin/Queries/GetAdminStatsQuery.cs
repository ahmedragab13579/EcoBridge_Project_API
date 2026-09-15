using EcoBridge.Domain.Results;
using EcoBridge.Application.DTO;
using EcoBridge.Application.Features.Statistics;
using MediatR;

namespace EcoBridge.Application.Features.Admin.Queries;

public record GetAdminStatsQuery : IRequest<Result<AdminStatsDTO>>;

public class GetAdminStatsQueryHandler(
    IStatisticsService statisticsService
) : IRequestHandler<GetAdminStatsQuery, Result<AdminStatsDTO>>
{
    public Task<Result<AdminStatsDTO>> Handle(GetAdminStatsQuery request, CancellationToken cancellationToken)
    {
        return statisticsService.GetAdminStatsAsync(cancellationToken);
    }
}
