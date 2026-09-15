using EcoBridge.Application.Contracts;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcoBridge.Application.Features.Donation.Commands;

public record CancelDonationCommand(
    Guid DonationId,
    Guid AccountId,
    string Reason
) : IRequest<Result<bool>>;

public class CancelDonationCommandHandler(
    IEcoBridgeDbContext dbContext
) : IRequestHandler<CancelDonationCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(CancelDonationCommand request, CancellationToken cancellationToken)
    {
        var donation = await dbContext.Donations
            .Include(d => d.Donor)
            .FirstOrDefaultAsync(d => d.Id == request.DonationId, cancellationToken);

        if (donation == null)
            return Result<bool>.FailResult(false, AppMessageCodes.DONATION_NOT_FOUND, ErrorType.NotFound);

        var account = await dbContext.Accounts.FirstOrDefaultAsync(a => a.Id == request.AccountId, cancellationToken);
        if (account == null)
            return Result<bool>.FailResult(false, AppMessageCodes.ACCOUNT_NOT_FOUND, ErrorType.NotFound);

        bool isAdmin = account.RoleId == UserRole.Admin;
        bool isDonorOwner = donation.DonorId == request.AccountId;
        bool isCharityOwner = donation.CharityId == request.AccountId;

        if (!isAdmin && !isDonorOwner && !isCharityOwner)
            return Result<bool>.FailResult(false, AppMessageCodes.FORBIDDEN, ErrorType.Forbidden);

        if (donation.Status == DonationStatus.Delivered || donation.Status == DonationStatus.Cancelled)
            return Result<bool>.FailResult(false, AppMessageCodes.DONATION_ALREADY_CANCELLED, ErrorType.Conflict);

        donation.Cancel(request.Reason);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result<bool>.FailResult(false, AppMessageCodes.CONCURRENCY_CONFLICT, ErrorType.Conflict);
        }

        return Result<bool>.SuccessResult(true, AppMessageCodes.DONATION_CANCELLED);
    }
}
