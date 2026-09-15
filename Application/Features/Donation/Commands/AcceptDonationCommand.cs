using EcoBridge.Application.Contracts;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcoBridge.Application.Features.Donation.Commands;

public record AcceptDonationCommand(
    Guid DonationId,
    Guid CharityId
) : IRequest<Result<bool>>;

public class AcceptDonationCommandHandler(
    IEcoBridgeDbContext dbContext
) : IRequestHandler<AcceptDonationCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(AcceptDonationCommand request, CancellationToken cancellationToken)
    {
        var donation = await dbContext.Donations.FirstOrDefaultAsync(d => d.Id == request.DonationId, cancellationToken);
        if (donation == null)
            return Result<bool>.FailResult(false, AppMessageCodes.DONATION_NOT_FOUND, ErrorType.NotFound);

        var charity = await dbContext.Charities
            .FirstOrDefaultAsync(c => c.AccountId == request.CharityId, cancellationToken);
        if (charity == null)
            return Result<bool>.FailResult(false, AppMessageCodes.CHARITY_NOT_FOUND, ErrorType.NotFound);

        if (donation.Status != DonationStatus.Pending && donation.Status != DonationStatus.Available)
            return Result<bool>.FailResult(false, AppMessageCodes.DONATION_ALREADY_ACCEPTED, ErrorType.Conflict);

        if (donation.CharityId != null)
            return Result<bool>.FailResult(false, AppMessageCodes.DONATION_ALREADY_REQUESTED, ErrorType.Conflict);

        donation.MarkAsAccepted(request.CharityId);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result<bool>.FailResult(false, AppMessageCodes.CONCURRENCY_CONFLICT, ErrorType.Conflict);
        }

        return Result<bool>.SuccessResult(true, AppMessageCodes.DONATION_ACCEPTED);
    }
}
