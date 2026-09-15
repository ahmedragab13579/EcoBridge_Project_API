using EcoBridge.Application.Contracts;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Results;
using MediatR;

namespace EcoBridge.Application.Features.Volunteer.Commands;

public record DeleteVolunteerCommand(Guid Id) : IRequest<Result<bool>>;

public class DeleteVolunteerCommandHandler(
    IEcoBridgeDbContext dbContext
) : IRequestHandler<DeleteVolunteerCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(DeleteVolunteerCommand request, CancellationToken cancellationToken)
    {
        var volunteer = await dbContext.Volunteers.FindAsync([request.Id], cancellationToken);
        if (volunteer == null)
            return Result<bool>.FailResult(false, AppMessageCodes.VOLUNTEER_NOT_FOUND, ErrorType.NotFound);

        dbContext.Volunteers.Remove(volunteer);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<bool>.SuccessResult(true, AppMessageCodes.VOLUNTEER_DELETED);
    }
}
