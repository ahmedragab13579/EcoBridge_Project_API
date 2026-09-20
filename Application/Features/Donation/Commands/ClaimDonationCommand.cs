using EcoBridge.Application.Contracts;
using EcoBridge.Application.Result;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcoBridge.Application.Features.Donation.Commands;

public record ClaimDonationCommand(
    Guid DonationId,
    Guid CharityId
) : IRequest<ApiResult<bool>>;

public class ClaimDonationCommandHandler(
    IEcoBridgeDbContext dbContext
) : IRequestHandler<ClaimDonationCommand, ApiResult<bool>>
{
    public async Task<ApiResult<bool>> Handle(
        ClaimDonationCommand request,
        CancellationToken cancellationToken)
    {
        var donation = await dbContext.Donations
            .FirstOrDefaultAsync(d => d.Id == request.DonationId, cancellationToken);

        if (donation == null)
            throw new DomainException.NotFoundException(AppMessageCodes.DONATION_NOT_FOUND);

        var charity = await dbContext.Accounts
            .FirstOrDefaultAsync(c => c.Id == request.CharityId, cancellationToken);

        if (charity == null)
            throw new DomainException.NotFoundException(AppMessageCodes.CHARITY_NOT_FOUND);

        if (donation.Status != DonationStatus.Pending && donation.Status != DonationStatus.Available)
            throw new DomainException.ConflictException(AppMessageCodes.DONATION_ALREADY_ACCEPTED);

        if (donation.CharityId != null)
            throw new DomainException.ConflictException(AppMessageCodes.DONATION_ALREADY_REQUESTED);

        donation.Claim(request.CharityId);

        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResult<bool>.Ok(true, AppMessageCodes.DONATION_ACCEPTED);
    }
}