using EcoBridge.Application.Contracts;
using EcoBridge.Application.DTO.WorkFlow;
using EcoBridge.Application.Features.Missions.Queries;
using EcoBridge.Application.Result;
using EcoBridge.Application.Results;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcoBridge.Application.Features.Missions.Handlers;

public class GetMyMissionsQueryHandler(
    IEcoBridgeDbContext dbContext
) : IRequestHandler<GetMyMissionsQuery, ApiResult<PagedResult<MissionDTO>>>
{
    public async Task<ApiResult<PagedResult<MissionDTO>>> Handle(
        GetMyMissionsQuery request,
        CancellationToken cancellationToken)
    {
        if (request.VolunteerAccountId == Guid.Empty)
            throw new DomainException.UnauthorizedException(AppMessageCodes.UNAUTHORIZED);

        var missions = await dbContext.DeliveryMissions
            .AsNoTracking()
            .Where(m => m.VolunteerId == request.VolunteerAccountId)
            .OrderByDescending(m => m.CreatedAt).Skip((request.page - 1) * request.pageSize).Take(request.pageSize)

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

        return ApiResult<PagedResult<MissionDTO>>.Ok(new PagedResult<MissionDTO>
        {
            Items = missions,
            Page = request.page,
            PageNumber = request.page,
            PageSize = request.pageSize,
            TotalCount = missions.Count,
           
        }, AppMessageCodes.SUCCESS);
    }
}