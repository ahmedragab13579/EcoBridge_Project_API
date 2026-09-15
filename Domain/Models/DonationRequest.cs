using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EcoBridge.Domain.Enums;

namespace EcoBridge.Domain.Models;

public class DonationRequest
{
    [Key]
    public Guid Id { get; protected set; }

    [Required]
    public Guid DonationId { get; protected set; }

    [Required]
    public Guid CharityId { get; protected set; }

    [Required]
    [Column(TypeName = "tinyint")]
    public RequestStatus RequestStatus { get; set; } = RequestStatus.Pending;

    [Required]
    [Column(TypeName = "datetime2")]
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;

    [ForeignKey(nameof(DonationId))]
    public Donation Donation { get; set; } = null!;

    [ForeignKey(nameof(CharityId))]
    public Charity Charity { get; set; } = null!;

    public DeliveryMission? DeliveryMission { get; set; }
    public ICollection<Review> Reviews { get; set; } = new List<Review>();

    protected DonationRequest() { }

    public static DonationRequest Create(Guid donationId, Guid charityId)
    {
        return new DonationRequest
        {
            Id = Guid.NewGuid(),
            DonationId = donationId,
            CharityId = charityId,
            RequestStatus = RequestStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Approve()
    {
        RequestStatus = RequestStatus.ApprovedByDonor;
    }

    public void Reject()
    {
        RequestStatus = RequestStatus.Rejected;
    }
}
