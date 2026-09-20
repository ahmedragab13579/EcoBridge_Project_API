using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Models.Mission;

namespace EcoBridge.Domain.Models.Donation;

public partial class Donation
{
    // ─────────────────────────────────────────────────────────────
    // Queries (pure)
    // ─────────────────────────────────────────────────────────────
    public bool IsExpired() => ExpiryDate <= DateTime.UtcNow;

    public bool IsTerminal() =>
        Status is DonationStatus.Delivered
            or DonationStatus.Cancelled
            or DonationStatus.Expired;

    public bool IsAvailableForClaim() =>
        !IsDeleted && Status == DonationStatus.Available && !IsExpired();

    public bool IsAssignedToVolunteer(Guid volunteerId) =>
        VolunteerId.HasValue && VolunteerId.Value == volunteerId;

    public bool IsOwnedByDonor(Guid donorId) => DonorId == donorId;

    public DeliveryMission? GetActiveMission() =>
        _deliveryMissions.FirstOrDefault(m => !m.IsClosed());
}