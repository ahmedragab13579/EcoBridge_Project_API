using EcoBridge.Application.Contracts;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Results;
using MediatR;

namespace EcoBridge.Application.Features.Volunteer.Commands;

public record ToggleVolunteerAvailabilityCommand(
    Guid AccountId,
    bool IsAvailable
) : IRequest<Result<bool>>;

public class ToggleVolunteerAvailabilityCommandHandler(
    IEcoBridgeDbContext dbContext
) : IRequestHandler<ToggleVolunteerAvailabilityCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(ToggleVolunteerAvailabilityCommand request, CancellationToken cancellationToken)
    {
        var volunteer = await dbContext.Volunteers.FindAsync([request.AccountId], cancellationToken);
        if (volunteer == null)
            return Result<bool>.FailResult(false, AppMessageCodes.VOLUNTEER_NOT_FOUND, ErrorType.NotFound);

        volunteer.SetAvailability(request.IsAvailable);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<bool>.SuccessResult(true, AppMessageCodes.VOLUNTEER_AVAILABILITY_UPDATED);
    }
}
