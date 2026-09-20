using EcoBridge.Application.Contracts;
using EcoBridge.Application.DTO;
using EcoBridge.Application.Features.Missions.Queries;
using EcoBridge.Application.Result;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcoBridge.Application.Features.Missions.Handlers;

public class GetMyMissionsQueryHandler(
    IEcoBridgeDbContext dbContext
) : IRequestHandler<GetMyMissionsQuery, ApiResult<List<MissionDTO>>>
{
    public async Task<ApiResult<List<MissionDTO>>> Handle(
        GetMyMissionsQuery request,
        CancellationToken cancellationToken)
    {
        if (request.VolunteerAccountId == Guid.Empty)
            throw new DomainException.UnauthorizedException(AppMessageCodes.UNAUTHORIZED);

        var missions = await dbContext.DeliveryMissions
            .AsNoTracking()
            .Where(m => m.VolunteerId == request.VolunteerAccountId)
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

        return ApiResult<List<MissionDTO>>.Ok(missions, AppMessageCodes.SUCCESS);
    }
}