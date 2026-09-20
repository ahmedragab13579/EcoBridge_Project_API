using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Exceptions;

namespace EcoBridge.Domain.Models.Mission;

public partial class DeliveryMission
{
    // ─────────────────────────────────────────────────────────────
    // Lifecycle — internal: driven by Donation only
    // ─────────────────────────────────────────────────────────────
    internal void ConfirmPickup()
    { if (IsDeleted)
            throw new DomainException.ConflictException(AppMessageCodes.MISSION_DELETED);

        if (DeliveryTime is not null)
            throw new DomainException.ConflictException(AppMessageCodes.MISSION_ALREADY_DELIVERED);

        if (CancelledAtUtc is not null)
            throw new DomainException.ConflictException(AppMessageCodes.MISSION_ALREADY_CANCELLED);

        if (PickupTime is not null)
            return; // idempotent

        PickupTime = DateTime.UtcNow;
        HangfireJobId = null;
    }

    internal void RecordDelivery()
    { if (IsDeleted)
            throw new DomainException.ConflictException(AppMessageCodes.MISSION_DELETED);

        if (CancelledAtUtc is not null)
            throw new DomainException.ConflictException(AppMessageCodes.MISSION_ALREADY_CANCELLED);

        if (PickupTime is null)
            throw new DomainException.ConflictException(AppMessageCodes.MISSION_NOT_IN_TRANSIT);

        if (DeliveryTime is not null)
            return; // idempotent

        DeliveryTime = DateTime.UtcNow;
        HangfireJobId = null;
        ClearOtp();
    }

    internal void Cancel(string? reason = null)
    {
        if (IsDeleted)
            throw new DomainException.ConflictException(AppMessageCodes.MISSION_DELETED);

        if (DeliveryTime is not null)
            throw new DomainException.ConflictException(AppMessageCodes.MISSION_ALREADY_DELIVERED);

        if (CancelledAtUtc is not null)
            return; // idempotent

        var trimmedReason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();

        if (trimmedReason is { Length: > MaxCancellationReasonLength })
            throw new DomainException.ValidationException(AppMessageCodes.CANCELLATION_REASON_TOO_LONG);

        CancelledAtUtc = DateTime.UtcNow;
        CancellationReason = trimmedReason;
        HangfireJobId = null;
        ClearOtp();
    }
}