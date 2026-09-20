using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EcoBridge.Domain.Contracts;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Exceptions;
using EcoBridge.Domain.Models.Accounts;
using EcoBridge.Domain.Models.Mission;

namespace EcoBridge.Domain.Models;

public class Review : ISoftDeletable
{
    // ─────────────────────────────────────────────────────────────
    // Business constants
    // ─────────────────────────────────────────────────────────────
    private const int MaxCommentLength = 500;

    // ─────────────────────────────────────────────────────────────
    // Properties — private setters
    // ─────────────────────────────────────────────────────────────
    [Key]
    public int Id { get; private set; }

    [Required]
    public Guid ReviewerId { get; private set; }

    [Required]
    public Guid RevieweeId { get; private set; }

    [Required]
    public int ReviewReasonId { get; private set; }

    /// <summary>
    /// The mission this review is attached to. Required — every review must
    /// be tied to a completed <see cref="DeliveryMission"/>.
    /// </summary>
    [Required]
    public Guid MissionId { get; private set; }

    [MaxLength(MaxCommentLength)]
    public string? Comment { get; private set; }

    [Required]
    [Column(TypeName = "datetime2")]
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    // ── Soft delete (ISoftDeletable) ─────────────────────────────
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    // ─────────────────────────────────────────────────────────────
    // Navigation properties
    // ─────────────────────────────────────────────────────────────
    [ForeignKey(nameof(ReviewerId))]
    public Account Reviewer { get; private set; } = null!;

    [ForeignKey(nameof(RevieweeId))]
    public Account Reviewee { get; private set; } = null!;

    [ForeignKey(nameof(ReviewReasonId))]
    public ReviewReason ReviewReason { get; private set; } = null!;

    [ForeignKey(nameof(MissionId))]
    public DeliveryMission Mission { get; private set; } = null!;

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

        var trimmedComment = NormalizeComment(comment);

        return new Review
        {
            ReviewerId = reviewerId,
            RevieweeId = revieweeId,
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

        if (trimmed is { Length: > MaxCommentLength })
            throw new DomainException.ValidationException(
                AppMessageCodes.REVIEW_COMMENT_TOO_LONG);

        return trimmed;
    }
}