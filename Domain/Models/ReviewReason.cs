using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EcoBridge.Domain.Constants;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Exceptions;

namespace EcoBridge.Domain.Models;

public class ReviewReason
{

    public int Id { get; private set; }


    public string Code { get; private set; } = string.Empty;


    public UserRole FromRole { get; private set; }


    public UserRole ToRole { get; private set; }

    public bool IsPositive { get; private set; }

    // ─────────────────────────────────────────────────────────────
    // Navigation properties
    // ─────────────────────────────────────────────────────────────
    private List<Review> _reviews = new List<Review>();
    public IReadOnlyCollection<Review> Reviews => _reviews;

    // ─────────────────────────────────────────────────────────────
    // EF Core constructor
    // ─────────────────────────────────────────────────────────────
    private ReviewReason() { }

    // ─────────────────────────────────────────────────────────────
    // Factory Method — the only valid creation path
    // ─────────────────────────────────────────────────────────────
    public static ReviewReason Create(
        string code,
        UserRole fromRole,
        UserRole toRole,
        bool isPositive)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new DomainException.ValidationException(
                AppMessageCodes.REVIEW_REASON_CODE_REQUIRED);

        if (!Enum.IsDefined(fromRole))
            throw new DomainException.ValidationException(
                AppMessageCodes.INVALID_REVIEW_REASON_FROM_ROLE);

        if (!Enum.IsDefined(toRole))
            throw new DomainException.ValidationException(
                AppMessageCodes.INVALID_REVIEW_REASON_TO_ROLE);

        if (fromRole == toRole)
            throw new DomainException.ValidationException(
                AppMessageCodes.REVIEW_REASON_SAME_ROLE_NOT_ALLOWED);

        var normalized = code.Trim().ToUpperInvariant();

        if (normalized.Length > DomainConstants.Name.MaxCodeLength)
            throw new DomainException.ValidationException(
                AppMessageCodes.REVIEW_REASON_CODE_TOO_LONG);

        if (!IsValidCodeFormat(normalized))
            throw new DomainException.ValidationException(
                AppMessageCodes.REVIEW_REASON_CODE_INVALID_FORMAT);

        return new ReviewReason
        {
            Code = normalized,
            FromRole = fromRole,
            ToRole = toRole,
            IsPositive = isPositive
        };
    }

    // ─────────────────────────────────────────────────────────────
    // Behavior: Update positivity (rare — e.g., re-tune the system)
    // ─────────────────────────────────────────────────────────────
    public void SetPolarity(bool isPositive)
    {
        IsPositive = isPositive;
    }

    // ─────────────────────────────────────────────────────────────
    // Domain queries (pure, no side effects)
    // ─────────────────────────────────────────────────────────────
    public bool IsNegative() => !IsPositive;

    public bool AppliesTo(UserRole from, UserRole to) =>
        FromRole == from && ToRole == to;

    public bool IsFrom(UserRole role) => FromRole == role;

    public bool IsTo(UserRole role) => ToRole == role;

    public bool IsInUse() => Reviews.Count > 0;

    // ─────────────────────────────────────────────────────────────
    // Invariants
    // ─────────────────────────────────────────────────────────────
    private static bool IsValidCodeFormat(string code)
    {
        foreach (var c in code)
        {
            var ok = (c >= 'A' && c <= 'Z')
                     || (c >= '0' && c <= '9')
                     || c == '_';
            if (!ok) return false;
        }
        return true;
    }
}