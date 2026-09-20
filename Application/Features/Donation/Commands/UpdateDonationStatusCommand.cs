using EcoBridge.Application.Contracts;
using EcoBridge.Application.DTO;
using EcoBridge.Application.Result;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcoBridge.Application.Features.Donation.Commands;

public record UpdateDonationStatusCommand(
    Guid DonationId,
    Guid VolunteerId,
    UpdateStatusDTO Dto
) : IRequest<ApiResult<bool>>;

public class UpdateDonationStatusCommandHandler(
    IEcoBridgeDbContext dbContext
) : IRequestHandler<UpdateDonationStatusCommand, ApiResult<bool>>
{
    public async Task<ApiResult<bool>> Handle(
        UpdateDonationStatusCommand request,
        CancellationToken cancellationToken)
    {
        var donation = await dbContext.Donations
            .FirstOrDefaultAsync(d => d.Id == request.DonationId, cancellationToken);

        if (donation == null)
            throw new DomainException.NotFoundException(AppMessageCodes.DONATION_NOT_FOUND);

        if (donation.VolunteerId != request.VolunteerId)
            throw new DomainException.ForbiddenException(AppMessageCodes.FORBIDDEN);

        if (!IsValidStatusTransition(donation.Status, request.Dto.Status))
            throw new DomainException.ValidationException(AppMessageCodes.INVALID_INPUT);

        if (request.Dto.Status == DonationStatus.InTransit)
            donation.SetInTransite();
        else if (request.Dto.Status == DonationStatus.Delivered)
            donation.SetAsDeleverd();
        else if (request.Dto.Status == DonationStatus.Cancelled)
            donation.Cancel("Status updated to cancelled");

        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResult<bool>.Ok(true, AppMessageCodes.DONATION_STATUS_UPDATED);
    }

    private static bool IsValidStatusTransition(DonationStatus current, DonationStatus next)
    {
        return (current == DonationStatus.Accepted && next == DonationStatus.InTransit)
            || (current == DonationStatus.InTransit && next == DonationStatus.Delivered);
    }
}