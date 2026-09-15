using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EcoBridge.Domain.Contracts;
using EcoBridge.Domain.Enums;

namespace EcoBridge.Domain.Models;

public class Donation : ISoftDeletable
{
    [Key]
    public Guid Id { get; protected set; }

    [Required]
    public Guid DonorId { get; protected set; }

    public int? CategoryId { get; protected set; }

    public int? CityId { get; protected set; }

    public Guid? CharityId { get; protected set; }
    public Guid? VolunteerId { get; protected set; }

    [MaxLength(200)]
    public string? Title { get; protected set; }

    [MaxLength(1000)]
    public string? Description { get; protected set; }

    [Required]
    [MaxLength(100)]
    public string FoodType { get; protected set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Quantity { get; protected set; } = string.Empty;

    [Column(TypeName = "decimal(10,2)")]
    public decimal? Weight { get; protected set; }

    [Required]
    [Column(TypeName = "datetime2")]
    public DateTime ExpiryDate { get; protected set; }

    [Required]
    [MaxLength(255)]
    public string PickupLocation { get; protected set; } = string.Empty;

    public string? ImageUrl { get; protected set; }

    [Required]
    [Column(TypeName = "tinyint")]
    public DonationStatus Status { get; set; }

    [Required]
    [Column(TypeName = "datetime2")]
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;

    [MaxLength(500)]
    public string? CancellationReason { get; protected set; }

    public bool IsDeleted { get; set; } = false;

    [Column(TypeName = "datetime2")]
    public DateTime? DeletedAt { get; set; }

    [Timestamp]
    public byte[] RowVersion { get; protected set; } = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

    [ForeignKey(nameof(CategoryId))]
    public Category? Category { get; set; }

    [ForeignKey(nameof(CityId))]
    public City? City { get; set; }

    public Donor Donor { get; set; } = null!;
    public Charity? Charity { get; set; }
    public Volunteer? Volunteer { get; set; }
    public ICollection<DonationRequest> DonationRequests { get; set; } = new List<DonationRequest>();

    protected Donation() { }

    public static Donation Create(
        Guid donorId,
        string foodType,
        string quantity,
        DateTime expiryDate,
        string pickupLocation,
        int? categoryId = null,
        string? title = null,
        string? description = null,
        decimal? weight = null,
        string? imageUrl = null,
        int? cityId = null)
    {
        return new Donation
        {
            Id = Guid.NewGuid(),
            DonorId = donorId,
            FoodType = foodType,
            Quantity = quantity,
            ExpiryDate = expiryDate,
            PickupLocation = pickupLocation,
            CategoryId = categoryId,
            CityId = cityId,
            Title = title,
            Description = description,
            Weight = weight,
            ImageUrl = imageUrl,
            Status = DonationStatus.Available,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false,
            DeletedAt = null
        };
    }

    public void UpdateDetails(
        string? title,
        string? description,
        string foodType,
        string quantity,
        decimal? weight,
        DateTime expiryDate,
        string pickupLocation,
        int? categoryId,
        string? imageUrl = null,
        int? cityId = null)
    {
        Title = title;
        Description = description;
        FoodType = foodType;
        Quantity = quantity;
        Weight = weight;
        ExpiryDate = expiryDate;
        PickupLocation = pickupLocation;
        CategoryId = categoryId;
        if (cityId.HasValue)
        {
            CityId = cityId;
        }
        if (imageUrl != null)
        {
            ImageUrl = imageUrl;
        }
    }

    public void MarkAsRequested()
    {
        Status = DonationStatus.Pending;
    }

    public void ReleaseCharityRequest()
    {
        CharityId = null;
        Status = DonationStatus.Available;
    }

    public void MarkAsAccepted(Guid charityId)
    {
        CharityId = charityId;
        Status = DonationStatus.Accepted;
    }

    public void AssignVolunteer(Guid volunteerId)
    {
        VolunteerId = volunteerId;
        Status = DonationStatus.InTransit;
    }

    public void MarkInTransit()
    {
        Status = DonationStatus.InTransit;
    }

    public void MarkAsDelivered()
    {
        Status = DonationStatus.Delivered;
    }

    public void Cancel(string reason)
    {
        Status = DonationStatus.Cancelled;
        CancellationReason = reason;
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
