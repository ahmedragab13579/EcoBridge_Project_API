using EcoBridge.Application.Contracts;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Results;
using EcoBridge.Application.DTO;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcoBridge.Application.Features.Volunteer.Queries;

public record GetVolunteerByIdQuery(Guid AccountId) : IRequest<Result<VolunteerDTO>>;

public class GetVolunteerByIdQueryHandler(
    IEcoBridgeDbContext dbContext
) : IRequestHandler<GetVolunteerByIdQuery, Result<VolunteerDTO>>
{
    public async Task<Result<VolunteerDTO>> Handle(GetVolunteerByIdQuery request, CancellationToken cancellationToken)
    {
        var volunteer = await dbContext.Volunteers
            .AsNoTracking()
            .Where(v => v.AccountId == request.AccountId)
            .Select(v => new VolunteerDTO
            {
                AccountId = v.AccountId,
                FullName = v.Account.FullName,
                VehicleDetails = v.VehicleDetails ?? string.Empty,
                IsAvailable = v.IsAvailable
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (volunteer == null)
            return Result<VolunteerDTO>.FailResult(new VolunteerDTO(), AppMessageCodes.VOLUNTEER_NOT_FOUND, ErrorType.NotFound);

        return Result<VolunteerDTO>.SuccessResult(volunteer, AppMessageCodes.VOLUNTEER_FETCHED);
    }
}
