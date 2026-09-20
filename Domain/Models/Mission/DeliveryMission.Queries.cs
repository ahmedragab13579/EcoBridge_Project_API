namespace EcoBridge.Domain.Models.Mission;

public partial class DeliveryMission
{
    // ─────────────────────────────────────────────────────────────
    // Queries
    // ─────────────────────────────────────────────────────────────
    public bool IsClosed() =>
        CancelledAtUtc is not null || DeliveryTime is not null || IsDeleted;

    public bool IsAssignedTo(Guid volunteerId) => VolunteerId == volunteerId;
}