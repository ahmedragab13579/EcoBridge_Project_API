using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EcoBridge.Domain.Constants;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Exceptions;
using EcoBridge.Domain.Models.Accounts;

namespace EcoBridge.Domain.Models;

public class PointTransaction
{

    public int Id { get; private set; }

    public Guid UserId { get; private set; }


    public decimal Amount { get; private set; }


    public TransactionType TransactionType { get; private set; }

    public Guid? ReferenceId { get; private set; }


    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;


    public Account User { get; private set; } = null!;


    private PointTransaction() { }


    public static PointTransaction Create(
        Guid userId,
        int amount,
        TransactionType transactionType,
        Guid? referenceId = null)
    {
        if (userId == Guid.Empty)
            throw new DomainException.ValidationException(
                AppMessageCodes.POINT_TRANSACTION_USER_ID_REQUIRED);

        if (amount == 0)
            throw new DomainException.ValidationException(
                AppMessageCodes.INVALID_TRANSACTION_AMOUNT);

        if (Math.Abs((long)amount) > DomainConstants.Financial.MaxAbsAmount)
            throw new DomainException.ValidationException(
                AppMessageCodes.TRANSACTION_AMOUNT_EXCEEDS_LIMIT);

        if (!Enum.IsDefined(transactionType))
            throw new DomainException.ValidationException(
                AppMessageCodes.INVALID_TRANSACTION_TYPE);

        if (referenceId == Guid.Empty)
            throw new DomainException.ValidationException(
                AppMessageCodes.INVALID_REFERENCE_ID);

        // ── Enforce sign per transaction type ────────────────────
        switch (transactionType)
        {
            case TransactionType.MissionCompleted:
                if (amount < 0)
                    throw new DomainException.ValidationException(
                        AppMessageCodes.MISSION_COMPLETED_MUST_BE_CREDIT);
                break;

            case TransactionType.Penalty:
                if (amount > 0)
                    throw new DomainException.ValidationException(
                        AppMessageCodes.PENALTY_MUST_BE_DEBIT);
                break;

            case TransactionType.PointsRedeemed:
                if (amount > 0)
                    throw new DomainException.ValidationException(
                        AppMessageCodes.REDEMPTION_MUST_BE_DEBIT);
                break;

            default:
                // Reached only if a new TransactionType is added and
                // not handled above — fail loud, not silent.
                throw new DomainException.ValidationException(
                    AppMessageCodes.UNSUPPORTED_TRANSACTION_TYPE);
        }

        return new PointTransaction
        {
            UserId = userId,
            Amount = amount,
            TransactionType = transactionType,
            ReferenceId = referenceId,
            CreatedAt = DateTime.UtcNow
        };
    }

    // ─────────────────────────────────────────────────────────────
    // Convenience factories (clearer call sites, safer defaults)
    // ─────────────────────────────────────────────────────────────
    public static PointTransaction CreateMissionReward(
        Guid userId, int points, Guid? missionId = null)
        => Create(userId, AbsSafe(points), TransactionType.MissionCompleted, missionId);

    public static PointTransaction CreatePenalty(
        Guid userId, int points, Guid? missionId = null)
        => Create(userId, -AbsSafe(points), TransactionType.Penalty, missionId);

    public static PointTransaction CreateRedemption(
        Guid userId, int points, Guid? referenceId = null)
        => Create(userId, -AbsSafe(points), TransactionType.PointsRedeemed, referenceId);

    // ─────────────────────────────────────────────────────────────
    // Domain queries (pure, no side effects)
    // ─────────────────────────────────────────────────────────────
    public bool IsCredit() => Amount > 0;

    public bool IsDebit() => Amount < 0;

    public bool BelongsTo(Guid userId) => UserId == userId;

    public bool References(Guid entityId) =>
        ReferenceId.HasValue && ReferenceId.Value == entityId;

    // ─────────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────────
    /// <summary>
    /// Returns the absolute value of <paramref name="value"/> without
    /// throwing on <see cref="int.MinValue"/>. Returns 0 in that case so
    /// the caller's validation rejects it as a zero amount.
    /// </summary>
    private static int AbsSafe(int value)
        => value == int.MinValue ? 0 : Math.Abs(value);
}