using EcoBridge.Application.Contracts;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Results;
using EcoBridge.Application.DTO;
using MediatR;

namespace EcoBridge.Application.Features.Volunteer.Commands;

public record UpdateVolunteerCommand(
    Guid Id,
    CreateVolunteerDTO Dto
) : IRequest<Result<bool>>;

public class UpdateVolunteerCommandHandler(
    IEcoBridgeDbContext dbContext
) : IRequestHandler<UpdateVolunteerCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(UpdateVolunteerCommand request, CancellationToken cancellationToken)
    {
        var volunteer = await dbContext.Volunteers.FindAsync([request.Id], cancellationToken);
        if (volunteer == null)
            return Result<bool>.FailResult(false, AppMessageCodes.VOLUNTEER_NOT_FOUND, ErrorType.NotFound);

        volunteer.UpdateVehicle(volunteer.VehicleType, request.Dto.VehicleDetails);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<bool>.SuccessResult(true, AppMessageCodes.VOLUNTEER_UPDATED);
    }
}
