using EcoBridge.Application.Contracts;
using EcoBridge.Application.Features.Events;
using EcoBridge.Application.Result;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Exceptions;
using EcoBridge.Domain.Models.Mission;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcoBridge.Application.Features.Donation.Commands;

public record AssignVolunteerCommand(
    Guid DonationId,
    Guid CallerId,
    Guid? TargetVolunteerId,
    bool IsAdmin
) : IRequest<ApiResult<bool>>;

public class AssignVolunteerCommandHandler(
    IEcoBridgeDbContext dbContext,
    IOutboxService outboxService
) : IRequestHandler<AssignVolunteerCommand, ApiResult<bool>>
{
    public async Task<ApiResult<bool>> Handle(
        AssignVolunteerCommand request,
        CancellationToken cancellationToken)
    {
        var donation = await dbContext.Donations
            .FirstOrDefaultAsync(d => d.Id == request.DonationId, cancellationToken);

        if (donation == null)
            throw new DomainException.NotFoundException(AppMessageCodes.DONATION_NOT_FOUND);

        if (donation.Status != DonationStatus.SearchingForVolunteer)
            throw new DomainException.ValidationException(AppMessageCodes.DONATION_CANNOT_BE_MODIFIED);

        if (donation.VolunteerId != null)
            throw new DomainException.ConflictException(AppMessageCodes.DONATION_ALREADY_HAS_ACTIVE_MISSION);

        Guid finalVolunteerId;
        if (request.IsAdmin)
        {
            if (!request.TargetVolunteerId.HasValue)
                throw new DomainException.ValidationException(AppMessageCodes.INVALID_INPUT);

            finalVolunteerId = request.TargetVolunteerId.Value;
        }
        else
        {
            finalVolunteerId = request.CallerId;
        }

        var volunteer = await dbContext.Accounts
            .FirstOrDefaultAsync(v => v.Id == finalVolunteerId, cancellationToken);

        if (volunteer == null)
            throw new DomainException.NotFoundException(AppMessageCodes.VOLUNTEER_NOT_FOUND);

        if (volunteer.Role != UserRole.Volunteer)
            throw new DomainException.ConflictException(AppMessageCodes.INVALID_ROLE);
        if (!volunteer.IsActive)
            throw new DomainException.ConflictException(AppMessageCodes.VOLUNTEER_INACTIVE);
        if (!volunteer.IsAvailable)
            throw new DomainException.ConflictException(AppMessageCodes.VOLUNTEER_NOT_AVAILABLE);

        // 1. Determine drop-off location from the claiming charity
        var dropOffLocation = "Charity Destination";
        if (donation.CharityId.HasValue)
        {
            var charity = await dbContext.Accounts
                .FirstOrDefaultAsync(c => c.Id == donation.CharityId.Value, cancellationToken);

            if (!string.IsNullOrWhiteSpace(charity?.AddressDetails))
            {
                dropOffLocation = charity.AddressDetails.Trim();
            }
            else if (!string.IsNullOrWhiteSpace(charity?.FullName))
            {
                dropOffLocation = charity.FullName.Trim();
            }
        }

        // 2. Transition donation state and lock volunteer availability
        donation.AssignVolunteer(finalVolunteerId);
        volunteer.SetAvailability(false);

        // 3. Create active delivery mission
        var mission = DeliveryMission.Create(
            donation.Id,
            finalVolunteerId,
            donation.PickupLocation,
            dropOffLocation);

        dbContext.DeliveryMissions.Add(mission);

        // 4. Enqueue durable mission accepted event for notification and SignalR broadcast
        outboxService.Enqueue(new MissionAcceptedEvent(
            mission.Id,
            finalVolunteerId,
            mission.PickupLocation));

        // 5. Commit state atomically
        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResult<bool>.Ok(true, AppMessageCodes.VOLUNTEER_ASSIGNED);
    }
}