using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Exceptions;

namespace EcoBridge.Domain.Models.Mission;

public partial class DeliveryMission
{
    // ─────────────────────────────────────────────────────────────
    // Timeout job wiring
    // ─────────────────────────────────────────────────────────────
    internal void AttachTimeoutJob(string hangfireJobId)
    {
        if (string.IsNullOrWhiteSpace(hangfireJobId))
            throw new DomainException.ValidationException(AppMessageCodes.HANGFIRE_JOB_ID_REQUIRED);
if(hangfireJobId.Length > 100)
            throw new DomainException.ValidationException(AppMessageCodes.HANGFIRE_JOB_ID_TOO_LONG);

        HangfireJobId = hangfireJobId.Trim();
    }

    internal void ClearTimeoutJob() => HangfireJobId = null;
}