using EcoBridge.Application.Contracts;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Results;
using EcoBridge.Application.DTO;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcoBridge.Application.Features.Volunteer.Queries;

public record GetVolunteersQuery : IRequest<Result<List<VolunteerDTO>>>;

public class GetVolunteersQueryHandler(
    IEcoBridgeDbContext dbContext
) : IRequestHandler<GetVolunteersQuery, Result<List<VolunteerDTO>>>
{
    public async Task<Result<List<VolunteerDTO>>> Handle(GetVolunteersQuery request, CancellationToken cancellationToken)
    {
        var volunteers = await dbContext.Volunteers
            .AsNoTracking()
            .Select(v => new VolunteerDTO
            {
                AccountId = v.AccountId,
                FullName = v.Account.FullName,
                VehicleDetails = v.VehicleDetails ?? string.Empty,
                IsAvailable = v.IsAvailable
            })
            .ToListAsync(cancellationToken);

        return Result<List<VolunteerDTO>>.SuccessResult(volunteers, AppMessageCodes.VOLUNTEER_FETCHED);
    }
}
