using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EcoBridge.Domain.Constants;
using EcoBridge.Domain.Contracts;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Exceptions;
using EcoBridge.Domain.Models.Accounts;
using EcoBridge.Domain.Models.Mission;

namespace EcoBridge.Domain.Models;

public class Review : ISoftDeletable
{
 
    public int Id { get; private set; }
    public int Rating { get; private set; }

    public Guid ReviewerId { get; private set; }

    public Guid RevieweeId { get; private set; }

    public int ReviewReasonId { get; private set; }


    public Guid MissionId { get; private set; }

    public string? Comment { get; private set; }

    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    // ── Soft delete (ISoftDeletable) ─────────────────────────────
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    // ─────────────────────────────────────────────────────────────
    // Navigation properties
    // ─────────────────────────────────────────────────────────────
    public Account Reviewer { get; private set; } = null!;

    public Account Reviewee { get; private set; } = null!;

    public ReviewReason ReviewReason { get; private set; } = null!;


    public DeliveryMission DeliveryMission { get; private set; }


    // ─────────────────────────────────────────────────────────────
    // EF Core constructor
    // ─────────────────────────────────────────────────────────────
    private Review() { }

    // ─────────────────────────────────────────────────────────────
    // Factory Method — the only valid creation path
    // ─────────────────────────────────────────────────────────────
    public static Review Create(
        Guid reviewerId,
        Guid revieweeId,
        int reating,
        int reviewReasonId,
        Guid missionId,
        string? comment = null)
    {
        if (reviewerId == Guid.Empty)
            throw new DomainException.ValidationException(
                AppMessageCodes.REVIEW_REVIEWER_ID_REQUIRED);

        if (revieweeId == Guid.Empty)
            throw new DomainException.ValidationException(
                AppMessageCodes.REVIEW_REVIEWEE_ID_REQUIRED);

        if (missionId == Guid.Empty)
            throw new DomainException.ValidationException(
                AppMessageCodes.REVIEW_MISSION_ID_REQUIRED);

        if (reviewerId == revieweeId)
            throw new DomainException.ValidationException(
                AppMessageCodes.REVIEW_CANNOT_REVIEW_SELF);

        if (reviewReasonId <= 0)
            throw new DomainException.ValidationException(
                AppMessageCodes.INVALID_REVIEW_REASON_ID);
        if (reating < DomainConstants.Lengths.MinReasonRatingNumber)
            throw new DomainException.ValidationException(
                AppMessageCodes.INVALID_REATING);
        if (reating > DomainConstants.Lengths.MaxReasonRatingNumber)
            throw new DomainException.ValidationException(
                AppMessageCodes.INVALID_REATING);

        var trimmedComment = NormalizeComment(comment);

        return new Review
        {
            ReviewerId = reviewerId,
            RevieweeId = revieweeId,
            Rating = reating,
            ReviewReasonId = reviewReasonId,
            MissionId = missionId,
            Comment = trimmedComment,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false,
            DeletedAt = null
        };
    }

    // ─────────────────────────────────────────────────────────────
    // Behavior: Update comment (only while not deleted)
    // ─────────────────────────────────────────────────────────────
    public void UpdateComment(string? comment)
    {
        EnsureNotDeleted();

        Comment = NormalizeComment(comment);
    }


    public void UpdateRating(int rating)
    {
        if (rating > DomainConstants.Lengths.MaxReasonRatingNumber || rating < DomainConstants.Lengths.MinReasonRatingNumber)
            throw new DomainException.ValidationException(AppMessageCodes.REVIEW_RATING_OUT_OF_RANGE);
        Rating = rating;
    }
    // ─────────────────────────────────────────────────────────────
    // Soft delete lifecycle
    // ─────────────────────────────────────────────────────────────
    public void SoftDelete()
    {
        if (IsDeleted) return;

        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
    }

    public void Restore()
    {
        if (!IsDeleted) return;

        IsDeleted = false;
        DeletedAt = null;
    }

    // ─────────────────────────────────────────────────────────────
    // Domain queries (pure, no side effects)
    // ─────────────────────────────────────────────────────────────
    public bool WasWrittenBy(Guid accountId) => ReviewerId == accountId;

    public bool WasWrittenAbout(Guid accountId) => RevieweeId == accountId;

    public bool Involves(Guid accountId) =>
        ReviewerId == accountId || RevieweeId == accountId;

    public bool HasComment() => !string.IsNullOrWhiteSpace(Comment);

    // ─────────────────────────────────────────────────────────────
    // Invariants
    // ─────────────────────────────────────────────────────────────
    private void EnsureNotDeleted()
    {
        if (IsDeleted)
            throw new DomainException.ConflictException(
                AppMessageCodes.REVIEW_DELETED);
    }

    private static string? NormalizeComment(string? comment)
    {
        var trimmed = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim();

        if (trimmed is { Length: > DomainConstants.Body.MaxBodyLength })
            throw new DomainException.ValidationException(
                AppMessageCodes.REVIEW_COMMENT_TOO_LONG);

        return trimmed;
    }
}