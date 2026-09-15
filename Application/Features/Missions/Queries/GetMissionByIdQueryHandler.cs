using EcoBridge.Application.Contracts;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Results;
using EcoBridge.Application.DTO;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcoBridge.Application.Features.Missions.Queries;

public class GetMissionByIdQueryHandler(
    IEcoBridgeDbContext dbContext
) : IRequestHandler<GetMissionByIdQuery, Result<MissionDTO>>
{
    public async Task<Result<MissionDTO>> Handle(GetMissionByIdQuery request, CancellationToken cancellationToken)
    {
        var mission = await dbContext.DeliveryMissions
            .AsNoTracking()
            .Where(m => m.Id == request.MissionId)
            .Select(m => new MissionDTO
            {
                Id = m.Id,
                DonationRequestId = m.DonationRequestId,
                VolunteerId = m.VolunteerId,
                VolunteerName = m.Volunteer != null && m.Volunteer.Account != null ? m.Volunteer.Account.FullName : null,
                MissionStatus = m.MissionStatus.ToString(),
                PickupLocation = m.PickupLocation,
                DropOffLocation = m.DropOffLocation,
                PickupTime = m.PickupTime,
                DeliveryTime = m.DeliveryTime,
                CreatedAt = m.CreatedAt,
                FoodType = m.DonationRequest != null && m.DonationRequest.Donation != null ? m.DonationRequest.Donation.FoodType : null,
                Quantity = m.DonationRequest != null && m.DonationRequest.Donation != null ? m.DonationRequest.Donation.Quantity : null
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (mission == null)
        {
            return Result<MissionDTO>.FailResult(new MissionDTO(), AppMessageCodes.MISSION_NOT_FOUND, ErrorType.NotFound);
        }

        return Result<MissionDTO>.SuccessResult(mission, AppMessageCodes.SUCCESS);
    }
}
