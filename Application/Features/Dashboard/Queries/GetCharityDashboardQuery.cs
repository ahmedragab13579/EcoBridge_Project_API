using EcoBridge.Application.Contracts;
using EcoBridge.Application.DTO.Statics.Dashboard;
using EcoBridge.Application.Result;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static System.Net.WebRequestMethods;

namespace EcoBridge.Application.Features.Dashboard.Queries;

public record GetCharityDashboardQuery(Guid CharityId) : IRequest<ApiResult<CharityDashboardDTO>>;

public class GetCharityDashboardQueryHandler(
    IEcoBridgeDbContext dbContext
) : IRequestHandler<GetCharityDashboardQuery, ApiResult<CharityDashboardDTO>>
{
    public async Task<ApiResult<CharityDashboardDTO>> Handle(
        GetCharityDashboardQuery request,
        CancellationToken ct)
    {
        if (request.CharityId == Guid.Empty)
            throw new DomainException.UnauthorizedException(AppMessageCodes.UNAUTHORIZED);

        // 1. Fetch Account TrustScore (Global query filter handles IsDeleted automatically)
        var trustScore = await dbContext.Accounts
            .AsNoTracking()
            .Where(a => a.Id == request.CharityId)
            .Select(a => (double?)a.TrustScore) // Nullable in case user not found
            .FirstOrDefaultAsync(ct);

        if (trustScore is null)
            throw new DomainException.NotFoundException(AppMessageCodes.USER_NOT_FOUND);

        // 2. Single grouped query for all donation statistics (including InTransit)
        var donationStats = await dbContext.DeliveryMissions
            .AsNoTracking()
            .Where(d => d.Donation.CharityId == request.CharityId)
            .GroupBy(d => d.Donation.CharityId)
            .Select(g => new
            {
                TotalFoodReceived = g.Count(d => d.Status == MissionStatus.Delivered),
                Failed = g.Count(d => d.Status == MissionStatus.Failed),
                Cancelled = g.Count(d => d.Status == MissionStatus.Cancelled),
                ActiveDeliveriesInTransit = g.Count(d => d.Status == MissionStatus.InTransit)
            })
            .FirstOrDefaultAsync(ct);

        var metrics = new CharityDashboardDTO
        {
            TotalFoodReceived = donationStats?.TotalFoodReceived ?? 0,
            CancelledDeliveries = donationStats?.Cancelled ?? 0,
            FailedDeliveries=donationStats?.Failed??0,
            ActiveDeliveriesInTransit = donationStats?.ActiveDeliveriesInTransit ?? 0,
            TrustScore =(int)trustScore.Value
        };

        return ApiResult<CharityDashboardDTO>.Ok(metrics, AppMessageCodes.DASHBOARD_RETRIEVED);
    }
}