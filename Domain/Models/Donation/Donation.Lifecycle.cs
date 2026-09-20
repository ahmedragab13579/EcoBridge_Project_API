using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Exceptions;
using EcoBridge.Domain.Models.Mission;

namespace EcoBridge.Domain.Models.Donation;

public partial class Donation
{
    // ─────────────────────────────────────────────────────────────
    // Lifecycle: Available -> Accepted
    // ─────────────────────────────────────────────────────────────
    public void Claim(Guid charityId)
    {
        EnsureNotDeleted();
        EnsureNotExpired();

        if (charityId == Guid.Empty)
            throw new DomainException.ValidationException(AppMessageCodes.CHARITY_ID_REQUIRED);

        if (Status != DonationStatus.Available)
            throw new DomainException.ConflictException(
                AppMessageCodes.DONATION_ALREADY_ACCEPTED);

        if (VolunteerId is not null || GetActiveMission() is not null)
            throw new DomainException.ConflictException(
                AppMessageCodes.DONATION_ALREADY_HAS_ACTIVE_MISSION);

        CharityId = charityId;
        Status = DonationStatus.Accepted;
    }


    public void SetInTransite()
    {
        if(Status!= DonationStatus.Accepted)
            throw new DomainException.ConflictException(
            AppMessageCodes.DONATION_NOT_ACCEPTED);
        Status = DonationStatus.InTransit;

    }
    public void SetAsDeleverd()
    {
        if (Status != DonationStatus.InTransit)
            throw new DomainException.ConflictException(
            AppMessageCodes.DONATION_INVALID_STATUS);
        Status = DonationStatus.Delivered;
    }

    public void StartVolunteerSearch()
    {
        EnsureNotDeleted();
        EnsureNotExpired();

        if (Status != DonationStatus.Accepted)
            throw new DomainException.ConflictException(
                AppMessageCodes.DONATION_NOT_ACCEPTED);

        if (VolunteerId is not null || GetActiveMission() is not null)
            throw new DomainException.ConflictException(
                AppMessageCodes.DONATION_ALREADY_HAS_ACTIVE_MISSION);

        Status = DonationStatus.SearchingForVolunteer;
    }

    // Accepted -> Available
    public void Unclaim()
    {
        EnsureNotDeleted();

        if (Status != DonationStatus.Accepted)
            throw new DomainException.ConflictException(
                AppMessageCodes.DONATION_NOT_ACCEPTED);

        if (VolunteerId is not null || GetActiveMission() is not null)
            throw new DomainException.ConflictException(
                AppMessageCodes.DONATION_ALREADY_HAS_ACTIVE_MISSION);

        CharityId = null;
        Status = DonationStatus.Available;
    }

    // ─────────────────────────────────────────────────────────────
    // Lifecycle: SearchingForVolunteer -> VolunteerAssigned
    // ─────────────────────────────────────────────────────────────
    public void AssignVolunteer(
        Guid volunteerId,
        string dropOffLocation)
    {
        EnsureNotDeleted();
        EnsureNotExpired();

        if (volunteerId == Guid.Empty)
            throw new DomainException.ValidationException(AppMessageCodes.VOLUNTEER_ID_REQUIRED);

        if (Status != DonationStatus.SearchingForVolunteer)
            throw new DomainException.ConflictException(
                AppMessageCodes.DONATION_NOT_SEARCHING_FOR_VOLUNTEER);

        if (GetActiveMission() is not null)
            throw new DomainException.ConflictException(
                AppMessageCodes.DONATION_ALREADY_HAS_ACTIVE_MISSION);

        var mission = DeliveryMission.Create(
            donationId: Id,
            volunteerId: volunteerId,
            pickupLocation: PickupLocation,
            dropOffLocation: dropOffLocation);

        _deliveryMissions.Add(mission);

        VolunteerId = volunteerId;
        Status = DonationStatus.VolunteerAssigned;
    }

    // VolunteerAssigned -> SearchingForVolunteer
    public void ReturnToVolunteerSearch()
    {
        EnsureNotDeleted();

        if (Status != DonationStatus.VolunteerAssigned)
            throw new DomainException.ConflictException(
                AppMessageCodes.DONATION_NOT_VOLUNTEER_ASSIGNED);

        if (GetActiveMission() is not null)
            throw new DomainException.ConflictException(
                AppMessageCodes.DONATION_ALREADY_HAS_ACTIVE_MISSION);

        VolunteerId = null;
        Status = DonationStatus.SearchingForVolunteer;
    }

    // VolunteerAssigned -> SearchingForVolunteer (timeout recovery)
    public void TimeoutVolunteerAssignment(Guid volunteerId, string? reason = null)
    {
        EnsureNotDeleted();

        if (Status != DonationStatus.VolunteerAssigned)
            throw new DomainException.ConflictException(
                AppMessageCodes.DONATION_NOT_VOLUNTEER_ASSIGNED);

        if (!VolunteerId.HasValue || VolunteerId.Value != volunteerId)
            throw new DomainException.ConflictException(
                AppMessageCodes.VOLUNTEER_NOT_FOUND);

        var activeMission = GetActiveMission();
        if (activeMission is not null)
        {
            var normalizedReason = string.IsNullOrWhiteSpace(reason)
                ? "Volunteer pickup timeout"
                : reason.Trim();

            activeMission.Cancel(normalizedReason);
        }

        VolunteerId = null;
        Status = DonationStatus.SearchingForVolunteer;
        ClearTimeoutJob();
    }

    // ─────────────────────────────────────────────────────────────
    // Lifecycle: VolunteerAssigned -> InTransit
    // ─────────────────────────────────────────────────────────────
    public void ConfirmPickup()
    {
        EnsureNotDeleted();
        EnsureNotExpired();

        if (Status != DonationStatus.VolunteerAssigned || !VolunteerId.HasValue)
            throw new DomainException.ConflictException(
                AppMessageCodes.DONATION_NOT_VOLUNTEER_ASSIGNED);

        var mission = RequireActiveMission();
        mission.ConfirmPickup();

        Status = DonationStatus.InTransit;
    }
}