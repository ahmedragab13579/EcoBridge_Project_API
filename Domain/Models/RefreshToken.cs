using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EcoBridge.Domain.Constants;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Exceptions;
using EcoBridge.Domain.Models.Accounts;

namespace EcoBridge.Domain.Models;

public class RefreshToken
{
    // ─────────────────────────────────────────────────────────────
    // Business constants
    // ─────────────────────────────────────────────────────────────



    public int Id { get; private set; }

    public string Token { get; private set; } = string.Empty;

    public Guid AccountId { get; private set; }

    public DateTime ExpiresAtUtc { get; private set; }

    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;

    public DateTime? RevokedAtUtc { get; private set; }

    public string? ReplacedByToken { get; private set; }

    public string? RevocationReason { get; private set; }

    // ─────────────────────────────────────────────────────────────
    // Navigation properties
    // ─────────────────────────────────────────────────────────────
    public Account Account { get; private set; } = null!;

    // ─────────────────────────────────────────────────────────────
    // EF Core constructor
    // ─────────────────────────────────────────────────────────────
    private RefreshToken() { }

    // ─────────────────────────────────────────────────────────────
    // Factory Method — the only valid creation path
    // ─────────────────────────────────────────────────────────────
    public static RefreshToken Create(
        Guid accountId,
        string token,
        DateTime? expiresAtUtc = null)
    {
        if (accountId == Guid.Empty)
            throw new DomainException.ValidationException(
                AppMessageCodes.REFRESH_TOKEN_ACCOUNT_ID_REQUIRED);

        if (string.IsNullOrWhiteSpace(token))
            throw new DomainException.ValidationException(
                AppMessageCodes.REFRESH_TOKEN_VALUE_REQUIRED);

        var trimmedToken = token.Trim();

        if (trimmedToken.Length > DomainConstants.Security.MaxTokenLength)
            throw new DomainException.ValidationException(
                AppMessageCodes.REFRESH_TOKEN_TOO_LONG);

        var expiry = expiresAtUtc ?? DateTime.UtcNow.Add(DomainConstants.Security. DefaultLifetime);

        if (expiry <= DateTime.UtcNow)
            throw new DomainException.ValidationException(
                AppMessageCodes.REFRESH_TOKEN_EXPIRY_IN_PAST);

        return new RefreshToken
        {
            AccountId = accountId,
            Token = trimmedToken,
            CreatedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = expiry
        };
    }

    // ─────────────────────────────────────────────────────────────
    // Behavior: Revoke (with optional replacement for rotation)
    // ─────────────────────────────────────────────────────────────
    public void Revoke(string? replacedByToken = null, string? reason = null)
    {
        if (RevokedAtUtc is not null) return; // idempotent

        // ── Validate everything BEFORE mutating any state ────────
        string? trimmedReplacement = null;
        if (replacedByToken is not null)
        {
            trimmedReplacement = replacedByToken.Trim();

            if (trimmedReplacement.Length == 0)
                throw new DomainException.ValidationException(
                    AppMessageCodes.REPLACEMENT_TOKEN_REQUIRED);

            if (trimmedReplacement.Length > DomainConstants.Security.MaxTokenLength)
                throw new DomainException.ValidationException(
                    AppMessageCodes.REPLACEMENT_TOKEN_TOO_LONG);

            if (string.Equals(trimmedReplacement, Token, StringComparison.Ordinal))
                throw new DomainException.ValidationException(
                    AppMessageCodes.REPLACEMENT_TOKEN_MATCHES_CURRENT);
        }

        string? trimmedReason = null;
        if (reason is not null)
        {
            trimmedReason = reason.Trim();

            if (trimmedReason.Length > DomainConstants.Security.MaxRevocationReasonLength)
                throw new DomainException.ValidationException(
                    AppMessageCodes.REVOCATION_REASON_TOO_LONG);

            if (trimmedReason.Length == 0)
                trimmedReason = null;
        }

        // ── Apply all mutations atomically ───────────────────────
        ReplacedByToken = trimmedReplacement;
        RevocationReason = trimmedReason;
        RevokedAtUtc = DateTime.UtcNow;
    }

    // ─────────────────────────────────────────────────────────────
    // Behavior: Mark as rotated (semantic alias for token rotation)
    // ─────────────────────────────────────────────────────────────
    public void Rotate(string newToken)
    {
        if (string.IsNullOrWhiteSpace(newToken))
            throw new DomainException.ValidationException(
                AppMessageCodes.NEW_TOKEN_REQUIRED);

        if (IsRevoked())
            throw new DomainException.ConflictException(
                AppMessageCodes.REFRESH_TOKEN_ALREADY_REVOKED);

        Revoke(newToken, DomainConstants.Security.RotationReason);
    }

    // ─────────────────────────────────────────────────────────────
    // Domain queries (pure, no side effects)
    // ─────────────────────────────────────────────────────────────
    public bool IsActive() =>
        RevokedAtUtc is null && ExpiresAtUtc > DateTime.UtcNow;

    public bool IsExpired() => ExpiresAtUtc <= DateTime.UtcNow;

    public bool IsRevoked() => RevokedAtUtc is not null;

    public bool WasReplaced() =>
        !string.IsNullOrWhiteSpace(ReplacedByToken);

    public bool BelongsTo(Guid accountId) => AccountId == accountId;

    public bool CanBeUsed() => IsActive();

    public bool IsReplayAttempt() => IsRevoked();
}