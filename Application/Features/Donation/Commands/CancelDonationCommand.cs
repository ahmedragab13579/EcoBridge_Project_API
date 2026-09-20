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
        var donation = await dbContext.Donations
            .Include(d => d.Donor)
            .FirstOrDefaultAsync(d => d.Id == request.DonationId, cancellationToken);

        if (donation == null)
            throw new DomainException.NotFoundException(AppMessageCodes.DONATION_NOT_FOUND);

        var account = await dbContext.Accounts
            .FirstOrDefaultAsync(a => a.Id == request.AccountId, cancellationToken);

        if (account == null)
            throw new DomainException.NotFoundException(AppMessageCodes.ACCOUNT_NOT_FOUND);

        bool isAdmin = account.RoleId == UserRole.Admin;
        bool isDonorOwner = donation.DonorId == request.AccountId;
        bool isCharityOwner = donation.CharityId == request.AccountId;

        if (!isAdmin && !isDonorOwner && !isCharityOwner)
            throw new DomainException.ForbiddenException(AppMessageCodes.FORBIDDEN);

        if (donation.Status == DonationStatus.Delivered || donation.Status == DonationStatus.Cancelled)
            throw new DomainException.ConflictException(AppMessageCodes.DONATION_ALREADY_CANCELLED);

        donation.Cancel(request.Reason);

        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResult<bool>.Ok(true, AppMessageCodes.DONATION_CANCELLED);
    }
}