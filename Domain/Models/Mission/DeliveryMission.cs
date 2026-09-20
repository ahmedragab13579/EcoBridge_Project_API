using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EcoBridge.Domain.Contracts;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Models.Accounts;

namespace EcoBridge.Domain.Models.Mission;

public partial class DeliveryMission : ISoftDeletable
{
    private const int MaxLocationLength = 255;
    private const int MaxCancellationReasonLength = 500;
    private const int OtpLength = 6;
    private const int MaxOtpAttempts = 5;
    private static readonly TimeSpan DefaultOtpLifetime = TimeSpan.FromMinutes(15);

    [Key]
    public Guid Id { get; private set; }

    [Required]
    public Guid DonationId { get; private set; }

    [Required]
    public Guid VolunteerId { get; private set; }

    [Required, MaxLength(MaxLocationLength)]
    public string PickupLocation { get; private set; } = string.Empty;

    [Required, MaxLength(MaxLocationLength)]
    public string DropOffLocation { get; private set; } = string.Empty;

    public DateTime? PickupTime { get; private set; }
    public DateTime? DeliveryTime { get; private set; }

    [MaxLength(OtpLength)]
    public string? OtpCode { get; private set; }

    public DateTime? OtpExpiresAtUtc { get; private set; }
    public int OtpAttemptCount { get; private set; }

    [MaxLength(100)]
    public string? HangfireJobId { get; private set; }

    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? CancelledAtUtc { get; private set; }

    [MaxLength(MaxCancellationReasonLength)]
    public string? CancellationReason { get; private set; }

    [Timestamp]
    public byte[] RowVersion { get; private set; } = Array.Empty<byte>();

    [ForeignKey(nameof(DonationId))]
    public Donation.Donation Donation { get; private set; } = null!;

    [ForeignKey(nameof(VolunteerId))]
    public Account Volunteer { get; private set; } = null!;

    private readonly List<Review> _reviews = new List<Review>();

    public IReadOnlyCollection <Review> Reviews =>  _reviews;

    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    private DeliveryMission() { }
}