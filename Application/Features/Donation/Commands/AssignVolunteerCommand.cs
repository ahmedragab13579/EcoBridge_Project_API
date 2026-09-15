using EcoBridge.Application.Contracts;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcoBridge.Application.Features.Donation.Commands;

public record AssignVolunteerCommand(
    Guid DonationId,
    Guid CallerId,
    Guid? TargetVolunteerId,
    bool IsAdmin
) : IRequest<Result<bool>>;

public class AssignVolunteerCommandHandler(
    IEcoBridgeDbContext dbContext
) : IRequestHandler<AssignVolunteerCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(AssignVolunteerCommand request, CancellationToken cancellationToken)
    {
        var donation = await dbContext.Donations.FirstOrDefaultAsync(d => d.Id == request.DonationId, cancellationToken);
        if (donation == null)
            return Result<bool>.FailResult(false, AppMessageCodes.DONATION_NOT_FOUND, ErrorType.NotFound);

        if (donation.Status != DonationStatus.Accepted)
            return Result<bool>.FailResult(false, AppMessageCodes.DONATION_CANNOT_BE_MODIFIED, ErrorType.Conflict);

        if (donation.VolunteerId != null)
            return Result<bool>.FailResult(false, AppMessageCodes.VOLUNTEER_ASSIGNED, ErrorType.Conflict);

        Guid finalVolunteerId;
        if (request.IsAdmin)
        {
            if (!request.TargetVolunteerId.HasValue)
                return Result<bool>.FailResult(false, AppMessageCodes.INVALID_INPUT, ErrorType.Validation);
            finalVolunteerId = request.TargetVolunteerId.Value;
        }
        else
        {
            finalVolunteerId = request.CallerId;
        }

        var volunteer = await dbContext.Volunteers.FirstOrDefaultAsync(v => v.AccountId == finalVolunteerId, cancellationToken);
        if (volunteer == null)
            return Result<bool>.FailResult(false, AppMessageCodes.VOLUNTEER_NOT_FOUND, ErrorType.NotFound);

        donation.AssignVolunteer(finalVolunteerId);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result<bool>.FailResult(false, AppMessageCodes.CONCURRENCY_CONFLICT, ErrorType.Conflict);
        }

        return Result<bool>.SuccessResult(true, AppMessageCodes.VOLUNTEER_ASSIGNED);
    }
}
