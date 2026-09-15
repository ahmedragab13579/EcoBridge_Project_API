using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EcoBridge.Domain.Contracts;

namespace EcoBridge.Domain.Models;

public class Review : ISoftDeletable
{
    [Key]
    public int Id { get; protected set; }

    [Required]
    public Guid ReviewerId { get; protected set; }

    [Required]
    public Guid RevieweeId { get; protected set; }

    [Required]
    public int ReviewReasonId { get; protected set; }

    public Guid? MissionId { get; protected set; }

    public Guid? RequestId { get; protected set; }

    [MaxLength(500)]
    public string? Comment { get; protected set; }

    [Required]
    [Column(TypeName = "datetime2")]
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;

    public bool IsDeleted { get; set; } = false;

    [Column(TypeName = "datetime2")]
    public DateTime? DeletedAt { get; set; }

    [ForeignKey(nameof(ReviewerId))]
    public Account Reviewer { get; set; } = null!;

    [ForeignKey(nameof(RevieweeId))]
    public Account Reviewee { get; set; } = null!;

    [ForeignKey(nameof(ReviewReasonId))]
    public ReviewReason ReviewReason { get; set; } = null!;

    [ForeignKey(nameof(MissionId))]
    public DeliveryMission? Mission { get; set; }

    [ForeignKey(nameof(RequestId))]
    public DonationRequest? Request { get; set; }

    protected Review() { }

    public static Review Create(Guid reviewerId, Guid revieweeId, int reviewReasonId, string? comment = null, Guid? missionId = null, Guid? requestId = null)
    {
        return new Review
        {
            ReviewerId = reviewerId,
            RevieweeId = revieweeId,
            ReviewReasonId = reviewReasonId,
            Comment = comment,
            MissionId = missionId,
            RequestId = requestId,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false,
            DeletedAt = null
        };
    }

    public void SoftDelete()
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
    }

    public void Restore()
    {
        IsDeleted = false;
        DeletedAt = null;
    }
}
