using System;
using System.Collections.Generic;
using System.Text;

namespace EcoBridge.Domain.Enums
{
    public enum OtpCheckResult
    {
        Valid = 0,
        NoOtpIssued = 1,
        Expired = 2,
        MaxAttemptsExceeded = 3,
        Invalid = 4,
        NotInTransit=5
    }
}
