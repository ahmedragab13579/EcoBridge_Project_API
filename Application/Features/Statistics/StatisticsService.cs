using EcoBridge.Application.Contracts;
using EcoBridge.Domain.Enums;
using EcoBridge.Application.DTO;
using EcoBridge.Domain.Results;
using EcoBridge.Domain.Errors;
using Microsoft.EntityFrameworkCore;

namespace EcoBridge.Application.Features.Statistics;

public class StatisticsService(IEcoBridgeDbContext dbContext) : IStatisticsService
{
    public async Task<Result<AdminStatsDTO>> GetAdminStatsAsync(CancellationToken cancellationToken = default)
    {
        var totalDonations = await dbContext.Donations.AsNoTracking().CountAsync(cancellationToken);
        var pendingDonations = await dbContext.Donations
            .AsNoTracking()
            .Where(d => d.Status == DonationStatus.Pending)
            .CountAsync(cancellationToken);
        var acceptedDonations = await dbContext.Donations
            .AsNoTracking()
            .Where(d => d.Status == DonationStatus.Accepted)
            .CountAsync(cancellationToken);
        var pickedUpDonations = await dbContext.Donations
            .AsNoTracking()
            .Where(d => d.Status == DonationStatus.InTransit)
            .CountAsync(cancellationToken);
        var deliveredDonations = await dbContext.Donations
            .AsNoTracking()
            .Where(d => d.Status == DonationStatus.Delivered)
            .CountAsync(cancellationToken);
        var cancelledDonations = await dbContext.Donations
            .AsNoTracking()
            .Where(d => d.Status == DonationStatus.Cancelled)
            .CountAsync(cancellationToken);

        var completedDeliveries = deliveredDonations;
        var totalVolunteers = await dbContext.Volunteers.AsNoTracking().CountAsync(cancellationToken);
        var activeDonors = await dbContext.Donors.AsNoTracking().CountAsync(cancellationToken);
        var totalCharities = await dbContext.Charities.AsNoTracking().CountAsync(cancellationToken);


        var result = new AdminStatsDTO
        {
            TotalDonations = totalDonations,
            TotalVolunteers = totalVolunteers,
            CompletedDeliveries = completedDeliveries,
            ActiveDonors = activeDonors,
            PendingDonations = pendingDonations,
            AcceptedDonations = acceptedDonations,
            PickedUpDonations = pickedUpDonations,
            DeliveredDonations = deliveredDonations,
            CancelledDonations = cancelledDonations,
            TotalCharities = totalCharities
        };
        return Result<AdminStatsDTO>.SuccessResult(result, AppMessageCodes.STATISTICS_RETRIEVED);
    }
}

