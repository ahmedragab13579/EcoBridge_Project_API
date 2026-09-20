namespace EcoBridge.Domain.Models.Mission;

public partial class DeliveryMission
{
    // ─────────────────────────────────────────────────────────────
    // Soft delete
    // ─────────────────────────────────────────────────────────────
    public void SoftDelete()
    {
        if (IsDeleted) return;
        CancelledAtUtc = DateTime.UtcNow;
        CancellationReason = "Mission deleted";
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        HangfireJobId = null;
        ClearOtp();
    }
}