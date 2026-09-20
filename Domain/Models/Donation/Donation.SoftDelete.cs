using EcoBridge.Domain.Enums;

namespace EcoBridge.Domain.Models.Donation;

public partial class Donation
{
    // ─────────────────────────────────────────────────────────────
    // Soft delete
    // ─────────────────────────────────────────────────────────────
    public void SoftDelete()
    {
        if (IsDeleted)
            return;

        // Close any live mission alongside the donation.
        GetActiveMission()?.Cancel("Donation soft-deleted.");
        Status = DonationStatus.Cancelled;
        CancelledAtUtc = DateTime.UtcNow;
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
    }
}