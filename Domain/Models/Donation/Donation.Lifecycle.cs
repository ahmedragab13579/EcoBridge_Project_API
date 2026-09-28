using EcoBridge.Domain.Constants;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Exceptions;

namespace EcoBridge.Domain.Models.Donation;

public partial class Donation
{
    public void Claim(Guid charityId)
    {
        EnsureNotDeleted();
        EnsureNotExpired();

        if (charityId == Guid.Empty)
            throw new DomainException.ValidationException(AppMessageCodes.CHARITY_ID_REQUIRED);

        if (Status != DonationStatus.Available)
            throw new DomainException.ConflictException(AppMessageCodes.DONATION_ALREADY_ACCEPTED);

        CharityId = charityId;
        Status = DonationStatus.SearchingForVolunteer;
    }

    public void Unclaim()
    {
        EnsureNotDeleted();

        if (Status != DonationStatus.SearchingForVolunteer)
            throw new DomainException.ConflictException(AppMessageCodes.DONATION_NOT_ACCEPTED);

        CharityId = null;
        Status = DonationStatus.Available;
    }

    public void AssignVolunteer(Guid volunteerId)
    {
        EnsureNotDeleted();
        EnsureNotExpired();

        if (volunteerId == Guid.Empty)
            throw new DomainException.ValidationException(AppMessageCodes.VOLUNTEER_ID_REQUIRED);

        if (Status != DonationStatus.SearchingForVolunteer)
            throw new DomainException.ConflictException(AppMessageCodes.DONATION_NOT_SEARCHING_FOR_VOLUNTEER);

        VolunteerId = volunteerId;
        Status = DonationStatus.VolunteerAssigned;
    }

    public void ReturnToVolunteerSearch()
    {
        EnsureNotDeleted();

        if (Status != DonationStatus.VolunteerAssigned)
            throw new DomainException.ConflictException(AppMessageCodes.DONATION_NOT_VOLUNTEER_ASSIGNED);

        VolunteerId = null;
        Status = DonationStatus.SearchingForVolunteer;
    }

    public void ConfirmPickup()
    {
        EnsureNotDeleted();
        EnsureNotExpired();

        if (Status != DonationStatus.VolunteerAssigned || !VolunteerId.HasValue)
            throw new DomainException.ConflictException(AppMessageCodes.DONATION_NOT_VOLUNTEER_ASSIGNED);

        Status = DonationStatus.Completed;
    }

  
    public void Cancel(string reason)
    {
        EnsureNotDeleted();

        if (Status == DonationStatus.Cancelled)
            return;

        if (Status is DonationStatus.Completed or DonationStatus.Expired)
            throw new DomainException.ConflictException(AppMessageCodes.DONATION_NOT_CANCELLABLE_IN_CURRENT_STATUS);

        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException.ValidationException(AppMessageCodes.DONATION_CANCELLATION_REASON_REQUIRED);

        var normalizedReason = reason.Trim();
        if (normalizedReason.Length >DomainConstants.DonationVal. MaxCancellationReasonLength)
            throw new DomainException.ValidationException(AppMessageCodes.DONATION_CANCELLATION_REASON_TOO_LONG);

        CharityId = null;
        VolunteerId = null;
        Status = DonationStatus.Cancelled;
        CancellationReason = normalizedReason;
        CancelledAtUtc = DateTime.UtcNow;
    }

    public void Expire()
    {
        if (IsDeleted)
            throw new DomainException.ConflictException(AppMessageCodes.DONATION_DELETED);

        if (IsTerminal())
            return;

        if (Status != DonationStatus.Available)
            throw new DomainException.ConflictException(AppMessageCodes.DONATION_NOT_EXPIRABLE_IN_CURRENT_STATUS);

        CharityId = null;
        VolunteerId = null;
        Status = DonationStatus.Expired;
        ExpiredAtUtc = DateTime.UtcNow;
    }

    public void SoftDelete()
    {
        if (IsDeleted)
            return;

        Status = DonationStatus.Cancelled;
        CancelledAtUtc = DateTime.UtcNow;
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
    }
}