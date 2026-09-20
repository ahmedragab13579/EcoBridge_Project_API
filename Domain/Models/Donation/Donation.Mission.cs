namespace EcoBridge.Domain.Models.Donation;

public partial class Donation
{
    // ─────────────────────────────────────────────────────────────
    // OTP — delegated to the active mission
    // ─────────────────────────────────────────────────────────────
    public string IssueOtp()
    {
        EnsureNotDeleted();
        EnsureInTransit();
        return RequireActiveMission().IssueOtp();
    }

    // ─────────────────────────────────────────────────────────────
    // Timeout job wiring
    // ─────────────────────────────────────────────────────────────
    public void AttachTimeoutJob(string hangfireJobId)
    {
        EnsureNotDeleted();
        RequireActiveMission().AttachTimeoutJob(hangfireJobId);
    }

    public void ClearTimeoutJob()
    {
        GetActiveMission()?.ClearTimeoutJob();
    }
}