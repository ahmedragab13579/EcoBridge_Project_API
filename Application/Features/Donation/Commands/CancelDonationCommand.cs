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

public record CancelDonationCommand(
    Guid DonationId,
    Guid AccountId,
    string Reason
) : IRequest<ApiResult<bool>>;

public class CancelDonationCommandHandler(
    IEcoBridgeDbContext dbContext,
    IHubContext<MissionHub> hubContext,
    INotificationService notificationService
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

        // 5. Handle Mission and Penalty (Exclude Admin from penalties)
        var mission = await dbContext.DeliveryMissions
            .FirstOrDefaultAsync(m => m.DonationId == request.DonationId &&
                                     (m.Status == MissionStatus.InTransit || m.Status == MissionStatus.WatingVoluneerArrival),
                                     cancellationToken);

        if (mission is not null)
        {
            if (!isAdmin)
            {
                var penalty = PenaltyLog.Create(account.Id, 50, "CANCEL_DONATION_FOR_ACTIVE_MISSION", mission.Id);
                await dbContext.PenaltyLogs.AddAsync(penalty, cancellationToken);
            }

            mission.SoftDelete();
        }

        // 6. Free assigned volunteer if present
        var volunteerId = donation.VolunteerId;
        if (volunteerId.HasValue)
        {
            var volunteer = await dbContext.Accounts
                .FirstOrDefaultAsync(a => a.Id == volunteerId.Value, cancellationToken);

            volunteer?.SetAvailability(true);
        }

        // 7. Execute Domain Cancellation logic
        donation.Cancel(request.Reason);

        await dbContext.SaveChangesAsync(cancellationToken);

        // 8. Send Notifications
        if (donation.DonorId != request.AccountId)
        {
            await notificationService.CreateNotificationAsync(
                donation.DonorId,
                "Donation Cancelled",
                $"Your donation has been cancelled. Reason: {request.Reason}",
                NotificationType.DonationStatusChanged,
                donation.Id,
                cancellationToken);
        }

        if (donation.CharityId.HasValue && donation.CharityId.Value != request.AccountId)
        {
            await notificationService.CreateNotificationAsync(
                donation.CharityId.Value,
                "Donation Cancelled",
                $"The donation has been cancelled. Reason: {request.Reason}",
                NotificationType.DonationStatusChanged,
                donation.Id,
                cancellationToken);
        }

        if (volunteerId.HasValue && volunteerId.Value != request.AccountId)
        {
            await notificationService.CreateNotificationAsync(
                volunteerId.Value,
                "Mission Cancelled",
                $"The donation mission you were assigned to was cancelled. Reason: {request.Reason}",
                NotificationType.DonationStatusChanged,
                donation.Id,
                cancellationToken);
        }

        // 9. SignalR broadcasts
        var payload = new
        {
            donationId = donation.Id,
            status = donation.Status.ToString(),
            reason = request.Reason
        };

        await hubContext.Clients.Group($"User_{donation.DonorId}")
            .SendAsync("DonationCancelled", payload, cancellationToken);

        if (donation.CharityId.HasValue)
        {
            await hubContext.Clients.Group($"User_{donation.CharityId.Value}")
                .SendAsync("DonationCancelled", payload, cancellationToken);
        }

        if (volunteerId.HasValue)
        {
            await hubContext.Clients.Group($"User_{volunteerId.Value}")
                .SendAsync("DonationCancelled", payload, cancellationToken);
        }

        return ApiResult<bool>.Ok(true, AppMessageCodes.DONATION_CANCELLED);
    }
}