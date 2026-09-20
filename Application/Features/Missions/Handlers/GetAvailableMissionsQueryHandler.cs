using EcoBridge.Application.Contracts;
using EcoBridge.Application.DTO;
using EcoBridge.Application.Features.Missions.Queries;
using EcoBridge.Application.Result;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Errors;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcoBridge.Application.Features.Missions.Handlers;

public class GetAvailableMissionsQueryHandler(
    IEcoBridgeDbContext dbContext
) : IRequestHandler<GetAvailableMissionsQuery, ApiResult<List<MissionDTO>>>
{
    public async Task<ApiResult<List<MissionDTO>>> Handle(
        GetAvailableMissionsQuery request,
        CancellationToken cancellationToken)
    {
        int? volunteerCityId = request.VolunteerCityId;
        int? volunteerGovId = null;

        if (!volunteerCityId.HasValue && request.VolunteerAccountId != Guid.Empty)
        {
            var accountInfo = await dbContext.Accounts
                .Where(a => a.Id == request.VolunteerAccountId)
                .Select(a => new
                {
                    a.CityId,
                    GovernorateId = (int?)(a.City != null ? a.City.GovernorateId : (int?)null)
                })
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
            .Where(m => m.Donation.Status == DonationStatus.SearchingForVolunteer);

        List<MissionDTO> missions;

        if (volunteerCityId.HasValue)
        {
            // First attempt: exact city match (Donation.CityId or Donor.CityId fallback)
            var cityMissions = await baseQuery
                .Where(m => (m.Donation.CityId ?? m.Donation.Donor.CityId) == volunteerCityId.Value)
                .OrderByDescending(m => m.CreatedAt)
                .Select(m => new MissionDTO
                {
                    Id = m.Id,
                    DonationRequestId = m.DonationId,
                    VolunteerId = m.VolunteerId,
                    MissionStatus = m.Donation.Status.ToString(),
                    PickupLocation = m.PickupLocation,
                    DropOffLocation = m.DropOffLocation,
                    PickupTime = m.PickupTime,
                    DeliveryTime = m.DeliveryTime,
                    CreatedAt = m.CreatedAt,
                    FoodType = m.Donation.FoodType,
                    Quantity = m.Donation.QuantityDescription
                })
                .ToListAsync(cancellationToken);

            if (cityMissions.Count > 0 || !volunteerGovId.HasValue)
            {
                missions = cityMissions;
            }
            else
            {
                // Fallback: governorate level
                missions = await baseQuery
                    .Where(m => (m.Donation.City != null
                                    ? m.Donation.City.GovernorateId
                                    : (m.Donation.Donor.City != null
                                        ? m.Donation.Donor.City.GovernorateId
                                        : (int?)null)) == volunteerGovId.Value)
                    .OrderByDescending(m => m.CreatedAt)
                    .Select(m => new MissionDTO
                    {
                        Id = m.Id,
                        DonationRequestId = m.DonationId,
                        VolunteerId = m.VolunteerId,
                        MissionStatus = m.Donation.Status.ToString(),
                        PickupLocation = m.PickupLocation,
                        DropOffLocation = m.DropOffLocation,
                        PickupTime = m.PickupTime,
                        DeliveryTime = m.DeliveryTime,
                        CreatedAt = m.CreatedAt,
                        FoodType = m.Donation.FoodType,
                        Quantity = m.Donation.QuantityDescription
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
                    DonationRequestId = m.DonationId,
                    VolunteerId = m.VolunteerId,
                    MissionStatus = m.Donation.Status.ToString(),
                    PickupLocation = m.PickupLocation,
                    DropOffLocation = m.DropOffLocation,
                    PickupTime = m.PickupTime,
                    DeliveryTime = m.DeliveryTime,
                    CreatedAt = m.CreatedAt,
                    FoodType = m.Donation.FoodType,
                    Quantity = m.Donation.QuantityDescription
                })
                .ToListAsync(cancellationToken);
        }

        return ApiResult<List<MissionDTO>>.Ok(missions, AppMessageCodes.SUCCESS);
    }
}