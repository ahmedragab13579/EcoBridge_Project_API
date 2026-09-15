using EcoBridge.Application.Contracts;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Results;
using EcoBridge.Application.DTO;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcoBridge.Application.Features.Missions.Queries;

public class GetAvailableMissionsQueryHandler(
    IEcoBridgeDbContext dbContext
) : IRequestHandler<GetAvailableMissionsQuery, Result<List<MissionDTO>>>
{
    public async Task<Result<List<MissionDTO>>> Handle(GetAvailableMissionsQuery request, CancellationToken cancellationToken)
    {
        int? volunteerCityId = request.VolunteerCityId;
        int? volunteerGovId = null;

        if (!volunteerCityId.HasValue && request.VolunteerAccountId != Guid.Empty)
        {
            var accountInfo = await dbContext.Accounts
                .Where(a => a.Id == request.VolunteerAccountId)
                .Select(a => new { a.CityId, GovernorateId = (int?)(a.City != null ? a.City.GovernorateId : (int?)null) })
                .FirstOrDefaultAsync(cancellationToken);

            if (accountInfo != null)
            {
                volunteerCityId = accountInfo.CityId;
                volunteerGovId = accountInfo.GovernorateId;
            }
        }
        else if (volunteerCityId.HasValue)
        {
            volunteerGovId = await dbContext.Cities
                .Where(c => c.Id == volunteerCityId.Value)
                .Select(c => (int?)c.GovernorateId)
                .FirstOrDefaultAsync(cancellationToken);
        }

        var baseQuery = dbContext.DeliveryMissions
            .AsNoTracking()
            .Where(m => m.MissionStatus == MissionStatus.SearchingForVolunteer);

        List<MissionDTO> missions;

        if (volunteerCityId.HasValue)
        {
            // First attempt: exact city match (using Donation.CityId or fallback to Donor.Account.CityId)
            var cityQuery = baseQuery.Where(m => (m.DonationRequest.Donation.CityId ?? m.DonationRequest.Donation.Donor.Account.CityId) == volunteerCityId.Value);

            var cityMissions = await cityQuery
                .OrderByDescending(m => m.CreatedAt)
                .Select(m => new MissionDTO
                {
                    Id = m.Id,
                    DonationRequestId = m.DonationRequestId,
                    VolunteerId = m.VolunteerId,
                    MissionStatus = m.MissionStatus.ToString(),
                    PickupLocation = m.PickupLocation,
                    DropOffLocation = m.DropOffLocation,
                    PickupTime = m.PickupTime,
                    DeliveryTime = m.DeliveryTime,
                    CreatedAt = m.CreatedAt,
                    FoodType = m.DonationRequest.Donation.FoodType,
                    Quantity = m.DonationRequest.Donation.Quantity
                })
                .ToListAsync(cancellationToken);

            if (cityMissions.Count > 0 || !volunteerGovId.HasValue)
            {
                missions = cityMissions;
            }
            else
            {
                // Fallback to governorate level
                missions = await baseQuery
                    .Where(m => (m.DonationRequest.Donation.City != null 
                        ? m.DonationRequest.Donation.City.GovernorateId 
                        : (m.DonationRequest.Donation.Donor.Account.City != null 
                            ? m.DonationRequest.Donation.Donor.Account.City.GovernorateId 
                            : (int?)null)) == volunteerGovId.Value)
                    .OrderByDescending(m => m.CreatedAt)
                    .Select(m => new MissionDTO
                    {
                        Id = m.Id,
                        DonationRequestId = m.DonationRequestId,
                        VolunteerId = m.VolunteerId,
                        MissionStatus = m.MissionStatus.ToString(),
                        PickupLocation = m.PickupLocation,
                        DropOffLocation = m.DropOffLocation,
                        PickupTime = m.PickupTime,
                        DeliveryTime = m.DeliveryTime,
                        CreatedAt = m.CreatedAt,
                        FoodType = m.DonationRequest.Donation.FoodType,
                        Quantity = m.DonationRequest.Donation.Quantity
                    })
                    .ToListAsync(cancellationToken);
            }
        }
        else
        {
            missions = await baseQuery
                .OrderByDescending(m => m.CreatedAt)
                .Select(m => new MissionDTO
                {
                    Id = m.Id,
                    DonationRequestId = m.DonationRequestId,
                    VolunteerId = m.VolunteerId,
                    MissionStatus = m.MissionStatus.ToString(),
                    PickupLocation = m.PickupLocation,
                    DropOffLocation = m.DropOffLocation,
                    PickupTime = m.PickupTime,
                    DeliveryTime = m.DeliveryTime,
                    CreatedAt = m.CreatedAt,
                    FoodType = m.DonationRequest.Donation.FoodType,
                    Quantity = m.DonationRequest.Donation.Quantity
                })
                .ToListAsync(cancellationToken);
        }

        return Result<List<MissionDTO>>.SuccessResult(missions, AppMessageCodes.SUCCESS);
    }
}
