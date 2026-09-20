using EcoBridge.Application.Contracts;
using EcoBridge.Application.DTO;
using EcoBridge.Application.Result;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

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
        var donationStats = await dbContext.Donations
            .AsNoTracking()
            .Where(d => d.CharityId == request.CharityId)
            .GroupBy(d => d.CharityId)
            .Select(g => new
            {
                TotalFoodReceived = g.Count(d => d.Status == DonationStatus.Delivered),
                PendingRequests = g.Count(d => d.Status == DonationStatus.Pending),
                ApprovedRequestsCount = g.Count(d => d.Status == DonationStatus.Accepted),
                ActiveDeliveriesInTransit = g.Count(d => d.Status == DonationStatus.InTransit)
            })
            .FirstOrDefaultAsync(ct);

        var metrics = new CharityDashboardDTO
        {
            TotalFoodReceived = donationStats?.TotalFoodReceived ?? 0,
            PendingRequests = donationStats?.PendingRequests ?? 0,
            ApprovedRequestsCount = donationStats?.ApprovedRequestsCount ?? 0,
            ActiveDeliveriesInTransit = donationStats?.ActiveDeliveriesInTransit ?? 0,
            TrustScore = trustScore.Value
        };

        return ApiResult<CharityDashboardDTO>.Ok(metrics, AppMessageCodes.DASHBOARD_RETRIEVED);
    }
}