using EcoBridge.Application.Contracts;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Results;
using EcoBridge.Application.DTO;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcoBridge.Application.Features.Missions.Queries;

public class GetMyMissionsQueryHandler(
    IEcoBridgeDbContext dbContext
) : IRequestHandler<GetMyMissionsQuery, Result<List<MissionDTO>>>
{
    public async Task<Result<List<MissionDTO>>> Handle(GetMyMissionsQuery request, CancellationToken cancellationToken)
    {
        if (request.VolunteerAccountId == Guid.Empty)
        {
            return Result<List<MissionDTO>>.FailResult([], AppMessageCodes.UNAUTHORIZED, ErrorType.Unauthorized);
        }

        var missions = await dbContext.DeliveryMissions
            .AsNoTracking()
            .Where(m => m.VolunteerId == request.VolunteerAccountId)
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

        return Result<List<MissionDTO>>.SuccessResult(missions, AppMessageCodes.SUCCESS);
    }
}
