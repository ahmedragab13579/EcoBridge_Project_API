using EcoBridge.Application.Contracts;
using EcoBridge.Application.Result;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcoBridge.Application.Features.Donation.Commands;

public record AssignVolunteerCommand(
    Guid DonationId,
    Guid CallerId,
    Guid? TargetVolunteerId,
    bool IsAdmin
) : IRequest<ApiResult<bool>>;

public class AssignVolunteerCommandHandler(
    IEcoBridgeDbContext dbContext
) : IRequestHandler<AssignVolunteerCommand, ApiResult<bool>>
{
    public async Task<ApiResult<bool>> Handle(
        AssignVolunteerCommand request,
        CancellationToken cancellationToken)
    {
        var donation = await dbContext.Donations
            .FirstOrDefaultAsync(d => d.Id == request.DonationId, cancellationToken);

        if (donation == null)
            throw new DomainException.NotFoundException(AppMessageCodes.DONATION_NOT_FOUND);

        if (donation.Status != DonationStatus.Accepted)
            throw new DomainException.ValidationException(AppMessageCodes.DONATION_CANNOT_BE_MODIFIED);

        if (donation.VolunteerId != null)
            throw new DomainException.ConflictException(AppMessageCodes.VOLUNTEER_ASSIGNED);

        Guid finalVolunteerId;
        if (request.IsAdmin)
        {
            if (!request.TargetVolunteerId.HasValue)
                throw new DomainException.ValidationException(AppMessageCodes.INVALID_INPUT);

            finalVolunteerId = request.TargetVolunteerId.Value;
        }
        else
        {
            finalVolunteerId = request.CallerId;
        }

        var volunteer = await dbContext.Accounts
            .FirstOrDefaultAsync(v => v.Id == finalVolunteerId, cancellationToken);

        if (volunteer == null)
            throw new DomainException.NotFoundException(AppMessageCodes.VOLUNTEER_NOT_FOUND);

        donation.AssignVolunteer(finalVolunteerId, "");

        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResult<bool>.Ok(true, AppMessageCodes.VOLUNTEER_ASSIGNED);
    }
}