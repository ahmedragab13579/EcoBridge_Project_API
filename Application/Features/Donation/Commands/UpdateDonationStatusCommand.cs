using EcoBridge.Application.Contracts;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Results;
using EcoBridge.Application.DTO;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcoBridge.Application.Features.Donation.Commands;

public record UpdateDonationStatusCommand(
    Guid DonationId,
    Guid VolunteerId,
    UpdateStatusDTO Dto
) : IRequest<Result<bool>>;

public class UpdateDonationStatusCommandHandler(
    IEcoBridgeDbContext dbContext
) : IRequestHandler<UpdateDonationStatusCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(UpdateDonationStatusCommand request, CancellationToken cancellationToken)
    {
        var donation = await dbContext.Donations.FirstOrDefaultAsync(d => d.Id == request.DonationId, cancellationToken);
        if (donation == null)
            return Result<bool>.FailResult(false, AppMessageCodes.DONATION_NOT_FOUND, ErrorType.NotFound);

        if (donation.VolunteerId != request.VolunteerId)
            return Result<bool>.FailResult(false, AppMessageCodes.FORBIDDEN, ErrorType.Forbidden);

        if (!IsValidStatusTransition(donation.Status, request.Dto.Status))
            return Result<bool>.FailResult(false, AppMessageCodes.INVALID_INPUT, ErrorType.Validation);

        if (request.Dto.Status == DonationStatus.InTransit)
            donation.MarkInTransit();
        else if (request.Dto.Status == DonationStatus.Delivered)
            donation.MarkAsDelivered();
        else if (request.Dto.Status == DonationStatus.Cancelled)
            donation.Cancel("Status updated to cancelled");

        await dbContext.SaveChangesAsync(cancellationToken);
        return Result<bool>.SuccessResult(true, AppMessageCodes.DONATION_STATUS_UPDATED);
    }

    private static bool IsValidStatusTransition(DonationStatus current, DonationStatus next)
    {
        return (current == DonationStatus.Accepted && next == DonationStatus.InTransit)
            || (current == DonationStatus.InTransit && next == DonationStatus.Delivered);
    }
}
