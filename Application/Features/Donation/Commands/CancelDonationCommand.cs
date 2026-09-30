using EcoBridge.Application.Contracts;
using EcoBridge.Application.Result;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcoBridge.Application.Features.Donation.Commands;

public record CancelDonationCommand(
    Guid DonationId,
    Guid AccountId,
    string Reason
) : IRequest<ApiResult<bool>>;

public class CancelDonationCommandHandler(
    IEcoBridgeDbContext dbContext
) : IRequestHandler<CancelDonationCommand, ApiResult<bool>>
{
    public async Task<ApiResult<bool>> Handle(
        CancelDonationCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Fetch donation
        var donation = await dbContext.Donations
            .FirstOrDefaultAsync(d => d.Id == request.DonationId, cancellationToken);

        if (donation is null)
            throw new DomainException.NotFoundException(AppMessageCodes.DONATION_NOT_FOUND);

        // 2. Fetch requesting account
        var account = await dbContext.Accounts
            .FirstOrDefaultAsync(a => a.Id == request.AccountId, cancellationToken);

        if (account is null)
            throw new DomainException.NotFoundException(AppMessageCodes.ACCOUNT_NOT_FOUND);

        // 3. Ownership and Role Check
        bool isAdmin = account.Role == UserRole.Admin;
        bool isDonorOwner = donation.DonorId == request.AccountId;
        bool isCharityOwner = donation.CharityId == request.AccountId;

        if (!isAdmin && !isDonorOwner && !isCharityOwner)
            throw new DomainException.ForbiddenException(AppMessageCodes.FORBIDDEN);

        // 4. Status Guards
        if (donation.Status == DonationStatus.Completed)
            throw new DomainException.ConflictException(AppMessageCodes.DONATION_CANNOT_BE_CANCELLED);

        if (donation.Status == DonationStatus.Cancelled)
            throw new DomainException.ConflictException(AppMessageCodes.DONATION_ALREADY_CANCELLED);

        // 5. Free assigned volunteer if present
        if (donation.VolunteerId.HasValue)
        {
            var volunteer = await dbContext.Accounts
                .FirstOrDefaultAsync(a => a.Id == donation.VolunteerId.Value, cancellationToken);

            volunteer?.SetAvailability(true);
        }

        // 6. Execute Domain Cancellation logic
        donation.Cancel(request.Reason);

        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResult<bool>.Ok(true, AppMessageCodes.DONATION_CANCELLED);
    }
}