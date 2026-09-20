using EcoBridge.Application.Contracts;
using EcoBridge.Application.Features.Donation.Commands;
using EcoBridge.Application.Result;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcoBridge.Application.Features.Donation.Handler;

public class RequestDonationCommandHandler(
    IEcoBridgeDbContext dbContext
) : IRequestHandler<RequestDonationCommand, ApiResult<Guid>>
{
    public async Task<ApiResult<Guid>> Handle(
        RequestDonationCommand request,
        CancellationToken cancellationToken)
    {
        var charity = await dbContext.Accounts
            .FirstOrDefaultAsync(c => c.Id == request.CharityAccountId, cancellationToken);

        if (charity == null)
            throw new DomainException.NotFoundException(AppMessageCodes.CHARITY_NOT_FOUND);

        var donation = await dbContext.Donations
            .FirstOrDefaultAsync(d => d.Id == request.DonationId, cancellationToken);

        if (donation == null)
            throw new DomainException.NotFoundException(AppMessageCodes.DONATION_NOT_FOUND);

        if (donation.CharityId != null)
            throw new DomainException.ConflictException(AppMessageCodes.DONATION_ALREADY_REQUESTED);

        if (donation.Status != DonationStatus.Available && donation.Status != DonationStatus.Pending)
            throw new DomainException.ValidationException(AppMessageCodes.DONATION_CANNOT_BE_MODIFIED);

        donation.Claim(charity.Id);

        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResult<Guid>.Ok(donation.Id, AppMessageCodes.DONATION_REQUEST_CREATED);
    }
}