using EcoBridge.Application.Contracts;
using EcoBridge.Application.DTO;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcoBridge.Application.Features.Dashboard.Queries;

public record GetDonorDashboardQuery(Guid DonorId) : IRequest<Result<DonorDashboardDTO>>;

public class GetDonorDashboardQueryHandler(
    IEcoBridgeDbContext dbContext
) : IRequestHandler<GetDonorDashboardQuery, Result<DonorDashboardDTO>>
{
    public async Task<Result<DonorDashboardDTO>> Handle(GetDonorDashboardQuery request, CancellationToken ct)
    {
        var account = await dbContext.Accounts
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == request.DonorId && !a.IsDeleted, ct);

        if (account == null)
        {
            return Result<DonorDashboardDTO>.FailResult(null!, AppMessageCodes.USER_NOT_FOUND, ErrorType.NotFound);
        }

        var donationStats = await dbContext.Donations
            .AsNoTracking()
            .Where(d => d.DonorId == request.DonorId && !d.IsDeleted)
            .GroupBy(x => 1) 
            .Select(g => new
            {
                TotalDonations = g.Count(),
                CompletedDonations = g.Count(d => d.Status == DonationStatus.Delivered)
            })
            .FirstOrDefaultAsync(ct);

        var pendingRequests = await dbContext.DonationRequests
            .AsNoTracking()
            .CountAsync(r => r.Donation.DonorId == request.DonorId && r.RequestStatus == RequestStatus.Pending, ct);

   
        var metrics = new DonorDashboardDTO
        {
            TotalDonationsCreated = donationStats?.TotalDonations ?? 0,
            TotalItemsSaved = donationStats?.CompletedDonations ?? 0, 
            TotalCompletedDonations = donationStats?.CompletedDonations ?? 0,
            ActivePendingRequests = pendingRequests,
            TotalPoints = account.TotalWalletPoints,
            TrustScore = account.TrustScore
        };

        return Result<DonorDashboardDTO>.SuccessResult(metrics, AppMessageCodes.DASHBOARD_RETRIEVED);
    }
}
