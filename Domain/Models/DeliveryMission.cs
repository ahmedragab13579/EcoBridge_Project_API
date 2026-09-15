using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EcoBridge.Domain.Enums;

namespace EcoBridge.Domain.Models;

public class DeliveryMission
{
    [Key]
    public Guid Id { get; protected set; }

    [Required]
    public Guid DonationRequestId { get; protected set; }

    public Guid? VolunteerId { get; set; }

    [Required]
    [Column(TypeName = "tinyint")]
    public MissionStatus MissionStatus { get; set; } = MissionStatus.SearchingForVolunteer;

    [Required]
    [MaxLength(255)]
    public string PickupLocation { get; protected set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string DropOffLocation { get; protected set; } = string.Empty;

    [Column(TypeName = "datetime2")]
    public DateTime? PickupTime { get; set; }

    [Column(TypeName = "datetime2")]
    public DateTime? DeliveryTime { get; set; }

    [MaxLength(10)]
    public string? OtpCode { get; protected set; }

    [Column(TypeName = "datetime2")]
    public DateTime? OtpExpiresAtUtc { get; protected set; }

    [MaxLength(100)]
    public string? HangfireJobId { get; set; }

    [Required]
    [Column(TypeName = "datetime2")]
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;

    [ForeignKey(nameof(DonationRequestId))]
    public DonationRequest DonationRequest { get; set; } = null!;

    [ForeignKey(nameof(VolunteerId))]
    public Volunteer? Volunteer { get; set; }

    public ICollection<Review> Reviews { get; set; } = new List<Review>();

    protected DeliveryMission() { }

    public static DeliveryMission Create(
        Guid donationRequestId,
        string pickupLocation,
        string dropOffLocation,
        string? otpCode = null,
        DateTime? otpExpiresAtUtc = null)
    {
        return new DeliveryMission
        {
            Id = Guid.NewGuid(),
            DonationRequestId = donationRequestId,
            PickupLocation = pickupLocation,
            DropOffLocation = dropOffLocation,
            MissionStatus = MissionStatus.SearchingForVolunteer,
            OtpCode = otpCode,
            OtpExpiresAtUtc = otpExpiresAtUtc,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Accept(Guid volunteerId, string? hangfireJobId = null)
    {
        VolunteerId = volunteerId;
        MissionStatus = MissionStatus.Accepted;
        HangfireJobId = hangfireJobId;
    }

    public void ConfirmPickup()
    {
        MissionStatus = MissionStatus.InTransit;
        PickupTime = DateTime.UtcNow;
        HangfireJobId = null;
    }

    public void Deliver()
    {
        MissionStatus = MissionStatus.Delivered;
        DeliveryTime = DateTime.UtcNow;
    }

    public void Cancel()
    {
        MissionStatus = MissionStatus.Cancelled;
    }

    public void SetOtp(string otpCode, DateTime expiresAtUtc)
    {
        OtpCode = otpCode;
        OtpExpiresAtUtc = expiresAtUtc;
    }
}
