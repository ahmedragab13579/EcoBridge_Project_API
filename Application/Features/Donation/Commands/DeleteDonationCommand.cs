using EcoBridge.Application.Contracts;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcoBridge.Application.Features.Donation.Commands;

public record DeleteDonationCommand(
    Guid DonationId,
    Guid AccountId
) : IRequest<Result<bool>>;

public class DeleteDonationCommandHandler(
    IEcoBridgeDbContext dbContext
) : IRequestHandler<DeleteDonationCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(DeleteDonationCommand request, CancellationToken cancellationToken)
    {
        var donor = await dbContext.Donors.FirstOrDefaultAsync(d => d.AccountId == request.AccountId, cancellationToken);
        if (donor == null)
            return Result<bool>.FailResult(false, AppMessageCodes.DONOR_NOT_FOUND, ErrorType.NotFound);

        var donation = await dbContext.Donations.FirstOrDefaultAsync(d => d.Id == request.DonationId, cancellationToken);
        if (donation == null)
            return Result<bool>.FailResult(false, AppMessageCodes.DONATION_NOT_FOUND, ErrorType.NotFound);

        if (donation.DonorId != donor.AccountId)
            return Result<bool>.FailResult(false, AppMessageCodes.FORBIDDEN, ErrorType.Forbidden);

        if (donation.Status != DonationStatus.Pending && donation.Status != DonationStatus.Available)
            return Result<bool>.FailResult(false, AppMessageCodes.DONATION_CANNOT_BE_MODIFIED, ErrorType.Conflict);

        donation.SoftDelete();
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result<bool>.SuccessResult(true, AppMessageCodes.DONATION_DELETED);
    }
}
