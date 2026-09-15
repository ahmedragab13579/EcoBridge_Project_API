using EcoBridge.Application.Contracts;
using EcoBridge.Application.DTO;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcoBridge.Application.Features.Dashboard.Queries;

public record GetCharityDashboardQuery(Guid CharityId) : IRequest<Result<CharityDashboardDTO>>;

public class GetCharityDashboardQueryHandler(
    IEcoBridgeDbContext dbContext
) : IRequestHandler<GetCharityDashboardQuery, Result<CharityDashboardDTO>>
{
    public async Task<Result<CharityDashboardDTO>> Handle(GetCharityDashboardQuery request, CancellationToken ct)
    {
        var account = await dbContext.Accounts
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == request.CharityId && !a.IsDeleted, ct);

        if (account == null)
        {
            return Result<CharityDashboardDTO>.FailResult(null!, AppMessageCodes.USER_NOT_FOUND, ErrorType.NotFound);
        }

        var requestStats = await dbContext.DonationRequests
            .AsNoTracking()
            .Where(r => r.CharityId == request.CharityId)
            .GroupBy(x => 1) 
            .Select(g => new
            {
                TotalFoodReceived = g.Count(r => r.RequestStatus == RequestStatus.ApprovedByDonor && r.Donation.Status == DonationStatus.Delivered),
                PendingRequests = g.Count(r => r.RequestStatus == RequestStatus.Pending),
                ApprovedRequestsCount = g.Count(r => r.RequestStatus == RequestStatus.ApprovedByDonor)
            })
            .FirstOrDefaultAsync(ct);

        var inTransitDeliveries = await dbContext.DeliveryMissions
            .AsNoTracking()
            .CountAsync(m => m.DonationRequest.CharityId == request.CharityId &&
                             (m.MissionStatus == MissionStatus.Accepted || m.MissionStatus == MissionStatus.InTransit), ct);

        var metrics = new CharityDashboardDTO
        {
            TotalFoodReceived = requestStats?.TotalFoodReceived ?? 0,
            PendingRequests = requestStats?.PendingRequests ?? 0,
            ApprovedRequestsCount = requestStats?.ApprovedRequestsCount ?? 0,
            ActiveDeliveriesInTransit = inTransitDeliveries,
            TrustScore = account.TrustScore
        };

        return Result<CharityDashboardDTO>.SuccessResult(metrics, AppMessageCodes.DASHBOARD_RETRIEVED);
    }
}
