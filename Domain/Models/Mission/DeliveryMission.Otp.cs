using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Exceptions;

namespace EcoBridge.Domain.Models.Mission;

public partial class DeliveryMission
{
    // ─────────────────────────────────────────────────────────────
    // OTP — commands
    // ─────────────────────────────────────────────────────────────
    internal string IssueOtp()
    {
        if (IsDeleted)
            throw new DomainException.ConflictException(AppMessageCodes.MISSION_DELETED);

        if (IsClosed())
            throw new DomainException.ConflictException(AppMessageCodes.MISSION_CLOSED);

        if (PickupTime is null)
            throw new DomainException.ConflictException(AppMessageCodes.MISSION_NOT_IN_TRANSIT);

        var code = Random.Shared.Next(0, 1_000_000).ToString("D6");
        SetOtp(code, DateTime.UtcNow.Add(DefaultOtpLifetime));
        return code;
    }

    internal void RegisterFailedOtpAttempt()
    {
         if (IsDeleted)
            throw new DomainException.ConflictException(AppMessageCodes.MISSION_DELETED);

        if (PickupTime is null)
            throw new DomainException.ConflictException(AppMessageCodes.MISSION_NOT_IN_TRANSIT);

        if (string.IsNullOrWhiteSpace(OtpCode) || OtpExpiresAtUtc is null)
            throw new DomainException.ConflictException(AppMessageCodes.OTP_NOT_ISSUED);

        if (OtpAttemptCount >= MaxOtpAttempts)
            throw new DomainException.ConflictException(AppMessageCodes.OTP_MAX_ATTEMPTS_EXCEEDED);

        OtpAttemptCount++;
    }

    internal void ConsumeOtp()
    {
        ClearOtp();
    }

    internal void VerifyAndConsumeOtp(string suppliedOtp)
    {
        var result = CheckOtp(suppliedOtp);

        switch (result)
        {
            case OtpCheckResult.Valid:
                ConsumeOtp();
                return;

            case OtpCheckResult.Invalid:
                // Only record the attempt; the switch below handles the throw.
                if (OtpAttemptCount < MaxOtpAttempts)
                    OtpAttemptCount++;
                break;
        }

        throw result switch
        {
            OtpCheckResult.NotInTransit =>
                new DomainException.ConflictException(AppMessageCodes.MISSION_NOT_IN_TRANSIT),

            OtpCheckResult.NoOtpIssued =>
                new DomainException.ConflictException(AppMessageCodes.OTP_NOT_ISSUED),

            OtpCheckResult.Expired =>
                new DomainException.ValidationException(AppMessageCodes.OTP_EXPIRED),

            OtpCheckResult.MaxAttemptsExceeded =>
                new DomainException.ConflictException(AppMessageCodes.OTP_MAX_ATTEMPTS_EXCEEDED),

            OtpCheckResult.Invalid =>
                new DomainException.ValidationException(AppMessageCodes.INVALID_OTP),

            _ => new DomainException.ValidationException(AppMessageCodes.INVALID_OTP)
        };
    }

    // ─────────────────────────────────────────────────────────────
    // OTP — pure query
    // ─────────────────────────────────────────────────────────────
    public OtpCheckResult CheckOtp(string? suppliedOtp)
    { if (IsDeleted)
            throw new DomainException.ConflictException(AppMessageCodes.MISSION_DELETED);

        if (PickupTime is null)
            return OtpCheckResult.NotInTransit;

        if (string.IsNullOrWhiteSpace(OtpCode) || OtpExpiresAtUtc is null)
            return OtpCheckResult.NoOtpIssued;

        if (DateTime.UtcNow > OtpExpiresAtUtc.Value)
            return OtpCheckResult.Expired;

        if (OtpAttemptCount >= MaxOtpAttempts)
            return OtpCheckResult.MaxAttemptsExceeded;

        if (!string.Equals(OtpCode, suppliedOtp?.Trim(), StringComparison.Ordinal))
            return OtpCheckResult.Invalid;

        return OtpCheckResult.Valid;
    }

    // ─────────────────────────────────────────────────────────────
    // OTP — internals
    // ─────────────────────────────────────────────────────────────
    private void SetOtp(string otpCode, DateTime expiresAtUtc)
    {
        if (string.IsNullOrWhiteSpace(otpCode))
            throw new DomainException.ValidationException(AppMessageCodes.OTP_CODE_REQUIRED);

        var code = otpCode.Trim();

        if (code.Length != OtpLength || code.Any(c => !char.IsDigit(c)))
            throw new DomainException.ValidationException(AppMessageCodes.INVALID_OTP_FORMAT);

        if (expiresAtUtc <= DateTime.UtcNow)
            throw new DomainException.ValidationException(AppMessageCodes.OTP_EXPIRY_IN_PAST);

        OtpCode = code;
        OtpExpiresAtUtc = expiresAtUtc;
        OtpAttemptCount = 0;
    }

    private void ClearOtp()
    {
        OtpCode = null;
        OtpExpiresAtUtc = null;
        OtpAttemptCount = 0;
    }
}