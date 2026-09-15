using EcoBridge.Application.Contracts;
using EcoBridge.Application.DTO;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcoBridge.Application.Features.Dashboard.Queries;

public record GetVolunteerDashboardQuery(Guid VolunteerId) : IRequest<Result<VolunteerDashboardDTO>>;

public class GetVolunteerDashboardQueryHandler(
    IEcoBridgeDbContext dbContext
) : IRequestHandler<GetVolunteerDashboardQuery, Result<VolunteerDashboardDTO>>
{
    public async Task<Result<VolunteerDashboardDTO>> Handle(GetVolunteerDashboardQuery request, CancellationToken ct)
    {
        var account = await dbContext.Accounts
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == request.VolunteerId && !a.IsDeleted, ct);

        if (account == null)
        {
            return Result<VolunteerDashboardDTO>.FailResult(null!, AppMessageCodes.USER_NOT_FOUND, ErrorType.NotFound);
        }

        var deliveryStats = await dbContext.DeliveryMissions
            .AsNoTracking()
            .Where(m => m.VolunteerId == request.VolunteerId)
            .GroupBy(x => 1)
            .Select(g => new
            {
                CompletedDeliveries = g.Count(m => m.MissionStatus == MissionStatus.Delivered),
                ActiveDeliveries = g.Count(m => m.MissionStatus == MissionStatus.Accepted || m.MissionStatus == MissionStatus.InTransit)
            })
            .FirstOrDefaultAsync(ct);

        var penaltyWarnings = await dbContext.PenaltyLogs
            .AsNoTracking()
            .CountAsync(p => p.AccountId == request.VolunteerId, ct);

        var metrics = new VolunteerDashboardDTO
        {
            CompletedDeliveries = deliveryStats?.CompletedDeliveries ?? 0,
            ActiveDeliveriesCount = deliveryStats?.ActiveDeliveries ?? 0,

            TotalPoints = account.TotalWalletPoints,
            ActivePenaltyWarnings = penaltyWarnings,
            TrustScore = account.TrustScore
        };

        return Result<VolunteerDashboardDTO>.SuccessResult(metrics, AppMessageCodes.DASHBOARD_RETRIEVED);
    }
}
