using EcoBridge.Application.Contracts;
using EcoBridge.Domain.Errors;
using EcoBridge.Application.DTO;
using MediatR;
using Microsoft.EntityFrameworkCore;
using EcoBridge.Application.Result;

namespace EcoBridge.Application.Features.Volunteer.Queries;

public record GetVolunteersQuery : IRequest<ApiResult<List<VolunteerDTO>>>;

public class GetVolunteersQueryHandler(
    IEcoBridgeDbContext dbContext
) : IRequestHandler<GetVolunteersQuery, ApiResult<List<VolunteerDTO>>>
{
    public async Task<ApiResult<List<VolunteerDTO>>> Handle(GetVolunteersQuery request, CancellationToken cancellationToken)
    {
        var volunteers = await dbContext.Accounts
            .AsNoTracking()
            .Where(v => v.RoleId == EcoBridge.Domain.Enums.UserRole.Volunteer)
            .Select(v => new VolunteerDTO
            {
                AccountId = v.Id,
                FullName = v.FullName,
                VehicleDetails = v.VehicleDetails ?? string.Empty,
                IsAvailable = v.IsAvailable
            })
            .ToListAsync(cancellationToken);

        return ApiResult<List<VolunteerDTO>>.Ok(volunteers, AppMessageCodes.VOLUNTEER_FETCHED);
    }
}
