using EcoBridge.Application.Contracts;
using EcoBridge.Application.DTO.WorkFlow;
using EcoBridge.Application.Features.Missions.Queries;
using EcoBridge.Application.Result;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcoBridge.Application.Features.Missions.Handlers;

public class GetMissionByIdQueryHandler(
    IEcoBridgeDbContext dbContext
) : IRequestHandler<GetMissionByIdQuery, ApiResult<MissionDTO>>
{
    public async Task<ApiResult<MissionDTO>> Handle(
        GetMissionByIdQuery request,
        CancellationToken cancellationToken)
    {
        var mission = await dbContext.DeliveryMissions
            .AsNoTracking()
            .Where(m => m.Id == request.MissionId)
            .Select(m => new MissionDTO
            {
                Id = m.Id,
                DonationRequestId = m.DonationId,
                VolunteerId = m.VolunteerId,
                VolunteerName = m.Volunteer != null ? m.Volunteer.FullName : null,
                MissionStatus = m.Donation.Status.ToString(),
                PickupLocation = m.PickupLocation,
                DropOffLocation = m.DropOffLocation,
                PickupTime = m.PickupTime,
                DeliveryTime = m.DeliveryTime,
                CreatedAt = m.CreatedAt,
                FoodType = m.Donation != null ? m.Donation.FoodType : null,
                Quantity = m.Donation != null ? m.Donation.QuantityDescription : null
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (mission == null)
            throw new DomainException.NotFoundException(AppMessageCodes.MISSION_NOT_FOUND);

        return ApiResult<MissionDTO>.Ok(mission, AppMessageCodes.SUCCESS);
    }
}