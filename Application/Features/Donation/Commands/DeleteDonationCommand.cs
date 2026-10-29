using EcoBridge.Application.Contracts;
using EcoBridge.Application.Features.Notifications;
using EcoBridge.Application.Hubs;
using EcoBridge.Application.Result;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Exceptions;
using EcoBridge.Domain.Models;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace EcoBridge.Application.Features.Donation.Commands;

public record DeleteDonationCommand(
    Guid DonationId,
    Guid AccountId
) : IRequest<ApiResult<bool>>;

public class DeleteDonationCommandHandler(
    IEcoBridgeDbContext dbContext,
    IHubContext<MissionHub> hubContext,
    INotificationService notificationService
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

        // Cannot delete completed donation
        if (donation.Status == DonationStatus.Completed)
            throw new DomainException.ConflictException(AppMessageCodes.DONATION_CANNOT_BE_CANCELLED);

        // If active mission exists
        var mission = await dbContext.DeliveryMissions
            .FirstOrDefaultAsync(m => m.DonationId == donation.Id &&
                                     (m.Status == MissionStatus.InTransit || m.Status == MissionStatus.WatingVoluneerArrival),
                                     cancellationToken);

        if (mission is not null)
        {
            var penalty = PenaltyLog.Create(donor.Id, 50, "DELETE_AN_ACTIVE_DONATION", mission.Id);
            await dbContext.PenaltyLogs.AddAsync(penalty, cancellationToken);
            mission.SoftDelete();
        }

        // Free volunteer if assigned
        var assignedVolunteerId = donation.VolunteerId;
        if (assignedVolunteerId.HasValue)
        {
            var volunteer = await dbContext.Accounts
                .FirstOrDefaultAsync(a => a.Id == assignedVolunteerId.Value, cancellationToken);

            volunteer?.SetAvailability(true);
        }

        var charityId = donation.CharityId;

        // Perform Soft Delete
        donation.SoftDelete();
        await dbContext.SaveChangesAsync(cancellationToken);

        // Send Notifications
        if (charityId.HasValue && charityId.Value != Guid.Empty)
        {
            await notificationService.CreateNotificationAsync(
                charityId.Value,
                "Donation Deleted",
                "The donor has deleted this donation.",
                NotificationType.DonationStatusChanged,
                donation.Id,
                cancellationToken);
        }

        if (assignedVolunteerId.HasValue)
        {
            await notificationService.CreateNotificationAsync(
                assignedVolunteerId.Value,
                "Mission Cancelled",
                "The donation for your active mission has been deleted by the donor.",
                NotificationType.DonationStatusChanged,
                donation.Id,
                cancellationToken);
        }

        // Real-time SignalR notifications
        var payload = new { donationId = donation.Id, status = "Deleted" };

        if (charityId.HasValue)
            await hubContext.Clients.Group($"User_{charityId.Value}").SendAsync("DonationDeleted", payload, cancellationToken);

        if (assignedVolunteerId.HasValue)
            await hubContext.Clients.Group($"User_{assignedVolunteerId.Value}").SendAsync("DonationDeleted", payload, cancellationToken);

        return ApiResult<bool>.Ok(true, AppMessageCodes.DONATION_DELETED);
    }
}