using EcoBridge.Application.Contracts;
using EcoBridge.Application.DTO.Statics.Dashboard;
using EcoBridge.Application.Result;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcoBridge.Application.Features.Dashboard.Queries;

public record GetDonorDashboardQuery(Guid DonorId) : IRequest<ApiResult<DonorDashboardDTO>>;

public class GetDonorDashboardQueryHandler(
    IEcoBridgeDbContext dbContext
) : IRequestHandler<GetDonorDashboardQuery, ApiResult<DonorDashboardDTO>>
{
    public async Task<ApiResult<DonorDashboardDTO>> Handle(
        GetDonorDashboardQuery request,
        CancellationToken ct)
    {
        if (request.DonorId == Guid.Empty)
            throw new DomainException.UnauthorizedException(AppMessageCodes.UNAUTHORIZED);

        // 1. Fetch only the required fields from Account
        var accountData = await dbContext.Accounts
            .AsNoTracking()
            .Where(a => a.Id == request.DonorId)
            .Select(a => new
            {
                a.TrustScore,
                a.TotalWalletPoints
            })
            .FirstOrDefaultAsync(ct);

        if (accountData is null)
            throw new DomainException.NotFoundException(AppMessageCodes.USER_NOT_FOUND);

        // 2. Aggregate donation statistics safely using GroupBy(d => d.DonorId)
        var donationStats = await dbContext.DeliveryMissions
            .AsNoTracking()
            .Where(d => d.Donation.DonorId == request.DonorId)
            .GroupBy(d => d.Donation.DonorId)
            .Select(g => new
            {
                TotalDonations = g.Count(),
                CompletedDonations = g.Count(d => d.Status == MissionStatus.Delivered)
            })
            .FirstOrDefaultAsync(ct);

        var metrics = new DonorDashboardDTO
        {
            TotalDonationsCreated = donationStats?.TotalDonations ?? 0,
            TotalItemsSaved = donationStats?.CompletedDonations ?? 0,
            TotalCompletedDonations = donationStats?.CompletedDonations ?? 0,
            TotalPoints = accountData.TotalWalletPoints,
            TrustScore = accountData.TrustScore
        };

        return ApiResult<DonorDashboardDTO>.Ok(metrics, AppMessageCodes.DASHBOARD_RETRIEVED);
    }
}