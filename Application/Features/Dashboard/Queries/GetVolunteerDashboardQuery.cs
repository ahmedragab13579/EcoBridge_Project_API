using EcoBridge.Application.Contracts;
using EcoBridge.Application.DTO.Statics.Dashboard;
using EcoBridge.Application.Result;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcoBridge.Application.Features.Dashboard.Queries;

public record GetVolunteerDashboardQuery(Guid VolunteerId) : IRequest<ApiResult<VolunteerDashboardDTO>>;

public class GetVolunteerDashboardQueryHandler(
    IEcoBridgeDbContext dbContext
) : IRequestHandler<GetVolunteerDashboardQuery, ApiResult<VolunteerDashboardDTO>>
{
    public async Task<ApiResult<VolunteerDashboardDTO>> Handle(
        GetVolunteerDashboardQuery request,
        CancellationToken ct)
    {
        if (request.VolunteerId == Guid.Empty)
            throw new DomainException.UnauthorizedException(AppMessageCodes.UNAUTHORIZED);

        // 1. Fetch only required Volunteer fields
        var accountData = await dbContext.Accounts
            .AsNoTracking()
            .Where(a => a.Id == request.VolunteerId)
            .Select(a => new
            {
                a.TrustScore,
                a.TotalWalletPoints
            })
            .FirstOrDefaultAsync(ct);

        if (accountData is null)
            throw new DomainException.NotFoundException(AppMessageCodes.USER_NOT_FOUND);

        // 2. Safe aggregation using GroupBy(m => m.VolunteerId)
        var deliveryStats = await dbContext.DeliveryMissions
            .AsNoTracking()
            .Where(m => m.VolunteerId == request.VolunteerId)
            .GroupBy(m => m.VolunteerId)
            .Select(g => new
            {
                CompletedDeliveries = g.Count(m => m.Status == MissionStatus.Delivered),
                ActiveDeliveries = g.Count(m =>
                    m.Donation.Status == DonationStatus.SearchingForVolunteer ||
                    m.Status == MissionStatus.InTransit)
            })
            .FirstOrDefaultAsync(ct);

        // 3. Count penalty logs
        var penaltyWarnings = await dbContext.PenaltyLogs
            .AsNoTracking()
            .CountAsync(p => p.AccountId == request.VolunteerId, ct);

        var metrics = new VolunteerDashboardDTO
        {
            CompletedDeliveries = deliveryStats?.CompletedDeliveries ?? 0,
            ActiveDeliveriesCount = deliveryStats?.ActiveDeliveries ?? 0,
            TotalPoints = accountData.TotalWalletPoints,
            ActivePenaltyWarnings = penaltyWarnings,
            TrustScore = accountData.TrustScore
        };

        return ApiResult<VolunteerDashboardDTO>.Ok(metrics, AppMessageCodes.DASHBOARD_RETRIEVED);
    }
}