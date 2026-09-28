using EcoBridge.Application.Contracts;
using EcoBridge.Application.Features.Donation.Commands;
using EcoBridge.Application.Features.Events;
using EcoBridge.Application.Features.Notifications;
using EcoBridge.Application.Hubs;
using EcoBridge.Application.Result;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace EcoBridge.Application.Features.Donation.Handler;

public class UnAssignVolunteerCommandHandler(
    IEcoBridgeDbContext dbContext,
    IPublisher publisher,
    IHubContext<MissionHub> hubContext,
    INotificationService notificationService
) : IRequestHandler<UnAssignVolunteerCommand, ApiResult<bool>>
{
    private const int UnassignPenaltyPoints = 10;
    private const string UnassignPenaltyReasonCode = "VOLUNTEER_UNASSIGNED";

    public async Task<ApiResult<bool>> Handle(
        UnAssignVolunteerCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Load donation with charity info (for SignalR + notification)
        var donation = await dbContext.Donations
            .FirstOrDefaultAsync(d => d.Id == request.DonationId, cancellationToken);

        if (donation == null)
            throw new DomainException.NotFoundException(AppMessageCodes.DONATION_NOT_FOUND);

        // 2. Only the assigned volunteer may unassign themselves
        if (donation.VolunteerId != request.VolunteerId)
            throw new DomainException.ForbiddenException(AppMessageCodes.FORBIDDEN);

        // 3. Only VolunteerAssigned can be unassigned
        if (donation.Status != DonationStatus.VolunteerAssigned)
            throw new DomainException.ConflictException(AppMessageCodes.DONATION_NOT_IN_VOLUNTEER_ASSIGNED);

        // 4. Free the volunteer
        var volunteer = await dbContext.Accounts
            .FirstOrDefaultAsync(a => a.Id == request.VolunteerId, cancellationToken);

        if (volunteer == null)
            throw new DomainException.NotFoundException(AppMessageCodes.VOLUNTEER_NOT_FOUND);

        volunteer.SetAvailability(true);

        // 5. Reset the donation back to SearchingForVolunteer
        donation.ReturnToVolunteerSearch();

        // 6. Persist state changes BEFORE publishing the penalty event
        await dbContext.SaveChangesAsync(cancellationToken);

        // 7. Publish the trust-score penalty (event handler creates PenaltyLog,
        //    decreases trust score, notifies, and triggers auto-ban if depleted)
        await publisher.Publish(new TrustScoreDeductedEvent(
            request.VolunteerId,
            UnassignPenaltyPoints,
            UnassignPenaltyReasonCode,
            Guid.Empty  // no mission yet at VolunteerAssigned stage
        ), cancellationToken);

        // 8. Notify the charity that a volunteer has bailed
        var charityId = donation.CharityId;
        if (charityId.HasValue && charityId.Value != Guid.Empty)
        {
            await notificationService.CreateNotificationAsync(
                charityId.Value,
                "Volunteer Unassigned",
                "Your assigned volunteer has unassigned from the mission. We are searching for a new volunteer.",
                NotificationType.DonationStatusChanged,
                donation.Id,
                cancellationToken);
        }

        // 9. SignalR: tell charity + volunteer to refetch
        var payload = new
        {
            donationId = donation.Id,
            status = donation.Status.ToString()
        };

        await hubContext.Clients.Group($"User_{request.VolunteerId}")
            .SendAsync("DonationUnassigned", payload, cancellationToken);

        if (charityId.HasValue && charityId.Value != Guid.Empty)
        {
            await hubContext.Clients.Group($"User_{charityId.Value}")
                .SendAsync("DonationUnassigned", payload, cancellationToken);
        }

        return ApiResult<bool>.Ok(true, AppMessageCodes.VOLUNTEER_UNASSIGNED);
    }
}