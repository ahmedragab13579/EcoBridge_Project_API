using EcoBridge.Application.Contracts;
using EcoBridge.Application.Result;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcoBridge.Application.Features.Donation.Commands;

public record DeleteDonationCommand(
    Guid DonationId,
    Guid AccountId
) : IRequest<ApiResult<bool>>;

public class DeleteDonationCommandHandler(
    IEcoBridgeDbContext dbContext
) : IRequestHandler<DeleteDonationCommand, ApiResult<bool>>
{
    public async Task<ApiResult<bool>> Handle(
        DeleteDonationCommand request,
        CancellationToken cancellationToken)
    {
        var donor = await dbContext.Accounts
            .FirstOrDefaultAsync(d => d.Id == request.AccountId, cancellationToken);

        if (donor == null)
            throw new DomainException.NotFoundException(AppMessageCodes.DONOR_NOT_FOUND);

        var donation = await dbContext.Donations
            .FirstOrDefaultAsync(d => d.Id == request.DonationId, cancellationToken);

        if (donation == null)
            throw new DomainException.NotFoundException(AppMessageCodes.DONATION_NOT_FOUND);

        if (donation.DonorId != donor.Id)
            throw new DomainException.ForbiddenException(AppMessageCodes.FORBIDDEN);

        if (donation.Status != DonationStatus.Pending && donation.Status != DonationStatus.Available)
            throw new DomainException.ValidationException(AppMessageCodes.DONATION_CANNOT_BE_MODIFIED);

        donation.SoftDelete();
        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResult<bool>.Ok(true, AppMessageCodes.DONATION_DELETED);
    }
}