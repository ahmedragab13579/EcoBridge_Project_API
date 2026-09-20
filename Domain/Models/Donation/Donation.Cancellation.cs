using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Exceptions;

namespace EcoBridge.Domain.Models.Donation;

public partial class Donation
{
    public void Cancel(string reason)
    {
        EnsureNotDeleted();

        if (Status == DonationStatus.Cancelled)
            return;

        if (Status is DonationStatus.Delivered or DonationStatus.Expired)
        {
            throw new DomainException.ConflictException(
                AppMessageCodes.DONATION_NOT_CANCELLABLE_IN_CURRENT_STATUS);
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new DomainException.ValidationException(
                AppMessageCodes.DONATION_CANCELLATION_REASON_REQUIRED);
        }

        var normalizedReason = reason.Trim();

        if (normalizedReason.Length > MaxCancellationReasonLength)
        {
            throw new DomainException.ValidationException(
                AppMessageCodes.DONATION_CANCELLATION_REASON_TOO_LONG);
        }

        GetActiveMission()?.Cancel(normalizedReason);

        CharityId = null;
        VolunteerId = null;
        Status = DonationStatus.Cancelled;
        CancellationReason = normalizedReason;
        CancelledAtUtc = DateTime.UtcNow;
    }
}