using EcoBridge.Application.Contracts;
using EcoBridge.Application.DTO;
using EcoBridge.Application.Result;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Errors;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcoBridge.Application.Features.Admin.Queries;

public record GetAdminStatsQuery : IRequest<ApiResult<AdminStatsDTO>>;

public class GetAdminStatsQueryHandler(
    IEcoBridgeDbContext dbContext
) : IRequestHandler<GetAdminStatsQuery, ApiResult<AdminStatsDTO>>
{
    public async Task<ApiResult<AdminStatsDTO>> Handle(
        GetAdminStatsQuery request,
        CancellationToken cancellationToken)
    {
        var donationStats = await dbContext.DeliveryMissions
            .AsNoTracking()
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Total = g.Count(),
                Available = g.Count(d => d.Donation.Status == DonationStatus.Available),
                Accepted = g.Count(d => d.Donation.Status == DonationStatus.SearchingForVolunteer),
                PickedUp = g.Count(d => d.Status == MissionStatus.InTransit),
                Delivered = g.Count(d => d.Status == MissionStatus.Delivered),
                CancelledD = g.Count(d => d.Status == MissionStatus.Cancelled),
                CancelledM = g.Count(d=>d.Donation.Status==DonationStatus.Cancelled)
            })
            .FirstOrDefaultAsync(cancellationToken);

        var accountStats = await dbContext.Accounts
            .AsNoTracking()
            .Where(a => !a.IsDeleted)
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Volunteers = g.Count(a => a.Role == UserRole.Volunteer),
                Donors = g.Count(a => a.Role == UserRole.Donor),
                Charities = g.Count(a => a.Role == UserRole.Charity)
            })
            .FirstOrDefaultAsync(cancellationToken);

        var result = new AdminStatsDTO
        {
            TotalDonations = donationStats?.Total ?? 0,
            AvailableDonations = donationStats?.Available ?? 0,
            AcceptedDonations = donationStats?.Accepted ?? 0,
            PickedUpDonations = donationStats?.PickedUp ?? 0,
            DeliveredDonations = donationStats?.Delivered ?? 0,
            CompletedDeliveries = donationStats?.Delivered ?? 0,
            CancelledDonations = donationStats?.CancelledD ?? 0,
            CancelledMissions = donationStats?.CancelledM ?? 0,
            TotalVolunteers = accountStats?.Volunteers ?? 0,
            ActiveDonors = accountStats?.Donors ?? 0,
            TotalCharities = accountStats?.Charities ?? 0
        };

        return ApiResult<AdminStatsDTO>.Ok(result, AppMessageCodes.STATISTICS_RETRIEVED);
    }
}