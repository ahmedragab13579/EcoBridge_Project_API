using EcoBridge.Application.Contracts;
using EcoBridge.Domain.Errors;
using EcoBridge.Application.DTO;
using MediatR;
using Microsoft.EntityFrameworkCore;
using EcoBridge.Application.Result;

namespace EcoBridge.Application.Features.Volunteer.Queries;

public record GetVolunteerByIdQuery(Guid AccountId) : IRequest<ApiResult<VolunteerDTO>>;

public class GetVolunteerByIdQueryHandler(
    IEcoBridgeDbContext dbContext
) : IRequestHandler<GetVolunteerByIdQuery, ApiResult<VolunteerDTO>>
{
    public async Task<ApiResult<VolunteerDTO>> Handle(GetVolunteerByIdQuery request, CancellationToken cancellationToken)
    {
        var volunteer = await dbContext.Accounts
            .AsNoTracking()
            .Where(v => v.Id == request.AccountId && v.RoleId == EcoBridge.Domain.Enums.UserRole.Volunteer)
            .Select(v => new VolunteerDTO
            {
                AccountId = v.Id,
                FullName = v.FullName,
                VehicleDetails = v.VehicleDetails ?? string.Empty,
                IsAvailable = v.IsAvailable
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (volunteer == null)
            return ApiResult<VolunteerDTO>.Fail(AppMessageCodes.VOLUNTEER_NOT_FOUND);

        return ApiResult<VolunteerDTO>.Ok(volunteer, AppMessageCodes.VOLUNTEER_FETCHED);
    }
}
