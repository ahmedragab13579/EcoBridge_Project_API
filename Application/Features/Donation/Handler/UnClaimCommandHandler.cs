using EcoBridge.Application.Contracts;
using EcoBridge.Application.Features.Donation.Commands;
using EcoBridge.Application.Features.Events;
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

namespace EcoBridge.Application.Features.Donation.Handler;

public class UnClaimCommandHandler(
    IEcoBridgeDbContext dbContext,
    IOutboxService outboxService,
    IHubContext<MissionHub> hubContext,
    INotificationService notificationService
) : IRequestHandler<UnClaimCommand, ApiResult<bool>>
{
    private const int CharityPenaltyLight = 5;
    private const int CharityPenaltyHeavy = 15;
    private const int VolunteerCompensationPoints = 25;

    private const string ReasonCharityUnclaimed = "CHARITY_UNCLAIMED";
    private const string ReasonCharityUnclaimedAfterAssignment = "CHARITY_UNCLAIMED_AFTER_ASSIGNMENT";

    public async Task<ApiResult<bool>> Handle(
        UnClaimCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Load donation
        var donation = await dbContext.Donations
            .FirstOrDefaultAsync(d => d.Id == request.DonationId, cancellationToken);

        if (donation == null)
            throw new DomainException.NotFoundException(AppMessageCodes.DONATION_NOT_FOUND);

        // 2. Only the owning charity may unclaim
        if (donation.CharityId != request.CharityId)
            throw new DomainException.ForbiddenException(AppMessageCodes.FORBIDDEN);

        // 3. Only two states are unclaimable
        var isSimpleUnclaim = donation.Status == DonationStatus.SearchingForVolunteer;
        var isAfterAssignment = donation.Status == DonationStatus.VolunteerAssigned;

        if (!isSimpleUnclaim && !isAfterAssignment)
            throw new DomainException.ConflictException(AppMessageCodes.DONATION_CANNOT_BE_UNCLAIMED);

        // 4. Capture volunteer (needed for compensation + SignalR)
        var volunteerId = donation.VolunteerId;

        // 5. Apply domain transition
        if (isAfterAssignment)
        {
            if (volunteerId.HasValue)
            {
                var volunteer = await dbContext.Accounts
                    .FirstOrDefaultAsync(a => a.Id == volunteerId.Value, cancellationToken);

                if (volunteer != null)
                {
                    volunteer.SetAvailability(true);
                    volunteer.AddPoints(VolunteerCompensationPoints);

                    var compensation = PointTransaction.Create(
                        volunteer.Id,
                        VolunteerCompensationPoints,
                        TransactionType.UnassignCompensation,
                        donation.Id);

                    dbContext.PointTransactions.Add(compensation);
                }
            }

            donation.Unclaim();
        }
        else
        {
            donation.Unclaim();
        }

        // 6. Enqueue durable trust penalty event for the charity
        outboxService.Enqueue(new TrustScoreDeductedEvent(
            request.CharityId,
            isAfterAssignment ? CharityPenaltyHeavy : CharityPenaltyLight,
            isAfterAssignment ? ReasonCharityUnclaimedAfterAssignment : ReasonCharityUnclaimed,
            Guid.Empty
        ));

        await dbContext.SaveChangesAsync(cancellationToken);

        // 7. Notify the donor
        await notificationService.CreateNotificationAsync(
            donation.DonorId,
            "Donation Available Again",
            "The charity has released your donation. It is now available for other charities.",
            NotificationType.DonationStatusChanged,
            donation.Id,
            cancellationToken);

        // 8. Notify the volunteer (if they were assigned)
        if (isAfterAssignment && volunteerId.HasValue)
        {
            await notificationService.CreateNotificationAsync(
                volunteerId.Value,
                "Mission Released",
                $"The charity has released the mission. You received {VolunteerCompensationPoints} wallet points as compensation.",
                NotificationType.DonationStatusChanged,
                donation.Id,
                cancellationToken);
        }

        // 9. SignalR — donor, charity, and (if applicable) volunteer
        var payload = new
        {
            donationId = donation.Id,
            status = donation.Status.ToString(),
            previousStatus = isAfterAssignment
                ? DonationStatus.VolunteerAssigned.ToString()
                : DonationStatus.SearchingForVolunteer.ToString()
        };

        await hubContext.Clients.Group($"User_{donation.DonorId}")
            .SendAsync("DonationUnclaimed", payload, cancellationToken);

        await hubContext.Clients.Group($"User_{request.CharityId}")
            .SendAsync("DonationUnclaimed", payload, cancellationToken);

        if (isAfterAssignment && volunteerId.HasValue)
        {
            await hubContext.Clients.Group($"User_{volunteerId.Value}")
                .SendAsync("DonationUnclaimed", payload, cancellationToken);
        }

        var successCode = isAfterAssignment
            ? AppMessageCodes.DONATION_UNCLAIMED_AFTER_ASSIGNMENT
            : AppMessageCodes.DONATION_UNCLAIMED;

        return ApiResult<bool>.Ok(true, successCode);
    }
}