using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Exceptions;

namespace EcoBridge.Domain.Models.Donation;

public partial class Donation
{
    // ─────────────────────────────────────────────────────────────
    // Expiration
    // ─────────────────────────────────────────────────────────────
    public void Expire()
    {
        if (IsDeleted)
            throw new DomainException.ConflictException(AppMessageCodes.DONATION_DELETED);

        if (IsTerminal())
            return;

        if (Status != DonationStatus.Available)
            throw new DomainException.ConflictException(
                AppMessageCodes.DONATION_NOT_EXPIRABLE_IN_CURRENT_STATUS);

        CharityId = null;
        VolunteerId = null;
        Status = DonationStatus.Expired;
        ExpiredAtUtc = DateTime.UtcNow;
    }
}