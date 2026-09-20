using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Exceptions;

namespace EcoBridge.Domain.Models.Mission;

public partial class DeliveryMission
{
    // ─────────────────────────────────────────────────────────────
    // Creation — internal: only Donation may create a mission
    // ─────────────────────────────────────────────────────────────
    internal static DeliveryMission Create(
        Guid donationId,
        Guid volunteerId,
        string pickupLocation,
        string dropOffLocation)
    {
        if (donationId == Guid.Empty)
            throw new DomainException.ValidationException(AppMessageCodes.DONATION_ID_REQUIRED);

        if (volunteerId == Guid.Empty)
            throw new DomainException.ValidationException(AppMessageCodes.VOLUNTEER_ID_REQUIRED);

        if (string.IsNullOrWhiteSpace(pickupLocation))
            throw new DomainException.ValidationException(AppMessageCodes.PICKUP_LOCATION_REQUIRED);

        if (string.IsNullOrWhiteSpace(dropOffLocation))
            throw new DomainException.ValidationException(AppMessageCodes.DROPOFF_LOCATION_REQUIRED);

        var pickup = pickupLocation.Trim();
        var dropOff = dropOffLocation.Trim();

        if (pickup.Length > MaxLocationLength)
            throw new DomainException.ValidationException(AppMessageCodes.PICKUP_LOCATION_TOO_LONG);

        if (dropOff.Length > MaxLocationLength)
            throw new DomainException.ValidationException(AppMessageCodes.DROPOFF_LOCATION_TOO_LONG);

        return new DeliveryMission
        {
            Id = Guid.NewGuid(),
            DonationId = donationId,
            VolunteerId = volunteerId,
            PickupLocation = pickup,
            DropOffLocation = dropOff,
            CreatedAt = DateTime.UtcNow
        };
    }
}