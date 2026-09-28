using EcoBridge.Domain.Constants;
using EcoBridge.Domain.Contracts;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Exceptions;
using EcoBridge.Domain.Models.Accounts;
using EcoBridge.Domain.Models.Common;

namespace EcoBridge.Domain.Models.Mission;

public partial class DeliveryMission : AggregateRoot, ISoftDeletable
{


    public Guid DonationId { get; private set; }
    public Guid VolunteerId { get; private set; }

    public MissionStatus Status { get; private set; }
    public string PickupLocation { get; private set; } = string.Empty;
    public string DropOffLocation { get; private set; } = string.Empty;

    public DateTime? PickupTime { get; private set; }
    public DateTime? DeliveryTime { get; private set; }

    public string? OtpCode { get; private set; }
    public DateTime? OtpExpiresAtUtc { get; private set; }
    public int OtpAttemptCount { get; private set; }

    public string? HangfireJobId { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? CancelledAtUtc { get; private set; }
    public string? CancellationReason { get; private set; }

    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public byte[] RowVersion { get; private set; } = Array.Empty<byte>();

    // Navigation Properties
    public Donation.Donation Donation { get; private set; } = null!;
    public Account Volunteer { get; private set; } = null!;

    private readonly List<Review> _reviews = new();
    public IReadOnlyCollection<Review> Reviews => _reviews.AsReadOnly();

    private DeliveryMission() { } // EF Core

    public static DeliveryMission Create(
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

        if (pickup.Length > DomainConstants.Body.MaxLocationLength)
            throw new DomainException.ValidationException(AppMessageCodes.PICKUP_LOCATION_TOO_LONG);

        if (dropOff.Length > DomainConstants.Body.MaxLocationLength)
            throw new DomainException.ValidationException(AppMessageCodes.DROPOFF_LOCATION_TOO_LONG);

        return new DeliveryMission
        {
            Id = Guid.NewGuid(),
            DonationId = donationId,
            VolunteerId = volunteerId,
            PickupLocation = pickup,
            DropOffLocation = dropOff,
            Status = MissionStatus.Assigned,
            CreatedAt = DateTime.UtcNow
        };
    }
}