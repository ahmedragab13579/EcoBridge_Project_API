using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Exceptions;
using EcoBridge.Domain.Models.Accounts;
using EcoBridge.Domain.Models.Mission;

namespace EcoBridge.Domain.Models;

public class PenaltyLog
{
    // ─────────────────────────────────────────────────────────────
    // Business constants
    // ─────────────────────────────────────────────────────────────
    private const int MaxReasonCodeLength = 100;

    // ─────────────────────────────────────────────────────────────
    // Properties — private setters (audit records are immutable)
    // ─────────────────────────────────────────────────────────────
    [Key]
    public int Id { get; private set; }

    [Required]
    public Guid AccountId { get; private set; }

    /// <summary>
    /// Points deducted. Always a positive integer — sign is implied by the
    /// record's existence, not by a negative value.
    /// </summary>
    [Required]
    public int Amount { get; private set; }

    [Required]
    [MaxLength(MaxReasonCodeLength)]
    public string ReasonCode { get; private set; } = string.Empty;

    public Guid? MissionId { get; private set; }

    [Required]
    [Column(TypeName = "datetime2")]
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;

    // ─────────────────────────────────────────────────────────────
    // Navigation properties
    // ─────────────────────────────────────────────────────────────
    [ForeignKey(nameof(AccountId))]
    public Account Account { get; private set; } = null!;

    [ForeignKey(nameof(MissionId))]
    public DeliveryMission? Mission { get; private set; }

    // ─────────────────────────────────────────────────────────────
    // EF Core constructor
    // ─────────────────────────────────────────────────────────────
    private PenaltyLog() { }

    // ─────────────────────────────────────────────────────────────
    // Factory Method — the only valid creation path
    // ─────────────────────────────────────────────────────────────
    public static PenaltyLog Create(
        Guid accountId,
        int amount,
        string reasonCode,
        Guid? missionId = null)
    {
        if (accountId == Guid.Empty)
            throw new DomainException.ValidationException(
                AppMessageCodes.PENALTY_ACCOUNT_ID_REQUIRED);

        if (string.IsNullOrWhiteSpace(reasonCode))
            throw new DomainException.ValidationException(
                AppMessageCodes.PENALTY_REASON_CODE_REQUIRED);

        if (amount <= 0)
            throw new DomainException.ValidationException(
                AppMessageCodes.INVALID_PENALTY_AMOUNT);

        var trimmedReason = reasonCode.Trim();

        if (trimmedReason.Length > MaxReasonCodeLength)
            throw new DomainException.ValidationException(
                AppMessageCodes.PENALTY_REASON_CODE_TOO_LONG);

        if (missionId == Guid.Empty)
            throw new DomainException.ValidationException(
                AppMessageCodes.INVALID_MISSION_ID);

        return new PenaltyLog
        {
            AccountId = accountId,
            Amount = amount,
            ReasonCode = trimmedReason,
            MissionId = missionId,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    // ─────────────────────────────────────────────────────────────
    // Domain queries (pure, no side effects)
    // ─────────────────────────────────────────────────────────────
    public bool BelongsTo(Guid accountId) => AccountId == accountId;

    public bool IsMissionRelated() => MissionId.HasValue;
}