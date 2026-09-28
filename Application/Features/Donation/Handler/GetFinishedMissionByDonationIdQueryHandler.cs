using EcoBridge.Application.Contracts;
using EcoBridge.Application.DTO;
using EcoBridge.Application.Extensions;
using EcoBridge.Application.Features.Donation.Queries;
using EcoBridge.Application.Result;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcoBridge.Application.Features.Donation.Handler;

public class GetFinishedMissionByDonationIdQueryHandler(
    IEcoBridgeDbContext dbContext
) : IRequestHandler<GetFinishedMissionByDonationIdQuery, ApiResult<DonationMissionDetailDTO>>
{
    public async Task<ApiResult<DonationMissionDetailDTO>> Handle(
        GetFinishedMissionByDonationIdQuery request,
        CancellationToken cancellationToken)
    {
        // 1. Load the donation (existence + ownership check)
        var donation = await dbContext.Donations
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == request.DonationId && !d.IsDeleted,
                cancellationToken);

        if (donation == null)
            throw new DomainException.NotFoundException(AppMessageCodes.DONATION_NOT_FOUND);

        // 2. Only the donor who created it may read the finished mission
        if (donation.DonorId != request.CallerId)
            throw new DomainException.ForbiddenException(AppMessageCodes.FORBIDDEN);

        // 3. Find the delivered mission (there is at most one per donation)
        var mission = await dbContext.DeliveryMissions
            .AsNoTracking()
            .Where(m => m.DonationId == request.DonationId
                     && m.DeliveryTime != null
                     && !m.IsDeleted)
            .Select(m => new
            {
                Mission = m,
                Volunteer = dbContext.Accounts
                    .Where(a => a.Id == m.VolunteerId)
                    .Select(a => new
                    {
                        a.Id,
                        a.FullName,
                        a.PhoneNumber,
                        a.ImagePath
                    })
                    .FirstOrDefault(),
                Charity = donation.CharityId != null
                    ? dbContext.Accounts
                        .Where(a => a.Id == donation.CharityId)
                        .Select(a => new
                        {
                            a.Id,
                            a.FullName,
                            a.OrganizationName,
                            a.AddressDetails
                        })
                        .FirstOrDefault()
                    : null
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (mission == null)
            throw new DomainException.NotFoundException(AppMessageCodes.MISSION_NOT_FOUND);

        // 4. Project to DTO
        var dto = new DonationMissionDetailDTO
        {
            // Mission
            MissionId = mission.Mission.Id,
            MissionStatus = donation.Status.ToString(),
            PickupTime = mission.Mission.PickupTime,
            DeliveryTime = mission.Mission.DeliveryTime,
            MissionCreatedAt = mission.Mission.CreatedAt,
            PickupLocation = mission.Mission.PickupLocation,
            DropOffLocation = mission.Mission.DropOffLocation,

            // Volunteer
            VolunteerId = mission.Volunteer?.Id ?? Guid.Empty,
            VolunteerName = mission.Volunteer?.FullName ?? string.Empty,
            VolunteerPhone = mission.Volunteer?.PhoneNumber,
            VolunteerImageUrl = mission.Volunteer?.ImagePath.ToCloudinaryUrl(),

            // Donation
            DonationId = donation.Id,
            DonationTitle = donation.Title,
            DonationDescription = donation.Description,
            FoodType = donation.FoodType,
            QuantityDes = donation.QuantityDescription,
            Weight = donation.WeightKg,
            DonationImageUrl = donation.ImageUrl.ToCloudinaryUrl(),

            // Charity
            CharityId = mission.Charity?.Id,
            CharityName = mission.Charity?.OrganizationName ?? mission.Charity?.FullName,
            CharityAddress = mission.Charity?.AddressDetails
        };

        return ApiResult<DonationMissionDetailDTO>.Ok(
            dto,
            AppMessageCodes.MISSION_RETRIEVED);
    }
}