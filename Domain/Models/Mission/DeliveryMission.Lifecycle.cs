using EcoBridge.Domain.Constants;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Exceptions;

namespace EcoBridge.Domain.Models.Mission;

public partial class DeliveryMission
{
    public void ConfirmPickup()
    {
        if (IsDeleted)
            throw new DomainException.ConflictException(AppMessageCodes.MISSION_DELETED);

        if (DeliveryTime is not null || Status == MissionStatus.Delivered)
            throw new DomainException.ConflictException(AppMessageCodes.MISSION_ALREADY_DELIVERED);

        if (CancelledAtUtc is not null || Status == MissionStatus.Cancelled)
            throw new DomainException.ConflictException(AppMessageCodes.MISSION_ALREADY_CANCELLED);

        if (PickupTime is not null)
            return; // Idempotent check

        PickupTime = DateTime.UtcNow;
        Status = MissionStatus.InTransit;
        IssueOtp();
        HangfireJobId = null;
    }
    public void Failed()
    {
        if (IsDeleted)
            throw new DomainException.ConflictException(AppMessageCodes.MISSION_DELETED);

        if (DeliveryTime is not null || Status == MissionStatus.Delivered)
            throw new DomainException.ConflictException(AppMessageCodes.MISSION_ALREADY_DELIVERED);

        if (CancelledAtUtc is not null || Status == MissionStatus.Cancelled)
            throw new DomainException.ConflictException(AppMessageCodes.MISSION_ALREADY_CANCELLED);

        if (PickupTime is not null)
            return; 

        PickupTime = DateTime.UtcNow;
        Status = MissionStatus.Failed;
        HangfireJobId = null;
    }

    public void RecordDelivery()
    {
        if (IsDeleted)
            throw new DomainException.ConflictException(AppMessageCodes.MISSION_DELETED);

        if (CancelledAtUtc is not null || Status == MissionStatus.Cancelled)
            throw new DomainException.ConflictException(AppMessageCodes.MISSION_ALREADY_CANCELLED);

        if (PickupTime is null)
            throw new DomainException.ConflictException(AppMessageCodes.MISSION_NOT_IN_TRANSIT);

        if (DeliveryTime is not null)
            return; // Idempotent check

        DeliveryTime = DateTime.UtcNow;
        Status = MissionStatus.Delivered;
        HangfireJobId = null;
        ClearOtp();
    }

    public void Cancel(string? reason = null)
    {
        if (IsDeleted)
            throw new DomainException.ConflictException(AppMessageCodes.MISSION_DELETED);

        if (DeliveryTime is not null || Status == MissionStatus.Delivered)
            throw new DomainException.ConflictException(AppMessageCodes.MISSION_ALREADY_DELIVERED);

        if (CancelledAtUtc is not null || Status == MissionStatus.Cancelled)
            return; // Idempotent check

        var trimmedReason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();

        if (trimmedReason is { Length: > DomainConstants.Body.MaxBodyLength })
            throw new DomainException.ValidationException(AppMessageCodes.CANCELLATION_REASON_TOO_LONG);

        CancelledAtUtc = DateTime.UtcNow;
        CancellationReason = trimmedReason;
        Status = MissionStatus.Cancelled;
        HangfireJobId = null;
        ClearOtp();
    }

    public void AttachTimeoutJob(string hangfireJobId)
    {
        if (string.IsNullOrWhiteSpace(hangfireJobId))
            throw new DomainException.ValidationException(AppMessageCodes.HANGFIRE_JOB_ID_REQUIRED);

        if (hangfireJobId.Length > DomainConstants.Body.MaxJobLength)
            throw new DomainException.ValidationException(AppMessageCodes.HANGFIRE_JOB_ID_TOO_LONG);

        HangfireJobId = hangfireJobId.Trim();
    }

    public void ClearTimeoutJob() => HangfireJobId = null;

    public void SoftDelete()
    {
        if (IsDeleted) return;

        Status = MissionStatus.Cancelled;
        CancelledAtUtc = DateTime.UtcNow;
        CancellationReason = "Mission soft-deleted";
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        HangfireJobId = null;
        ClearOtp();
    }
}