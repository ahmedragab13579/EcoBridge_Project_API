using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EcoBridge.Domain.Contracts;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Models.Accounts;
using EcoBridge.Domain.Models.Mission;

namespace EcoBridge.Domain.Models.Donation;

public partial class Donation : ISoftDeletable
{
    private const int MaxTitleLength = 200;
    private const int MaxDescriptionLength = 1000;
    private const int MaxFoodTypeLength = 100;
    private const int MaxQuantityLength = 50;
    private const int MaxPickupLocationLength = 255;
    private const int MaxCancellationReasonLength = 500;
    private const decimal MaxWeightKg = 100_000m;

    [Key]
    public Guid Id { get; private set; }

    [Required]
    public Guid DonorId { get; private set; }

    public Guid? CharityId { get; private set; }
    public Guid? VolunteerId { get; private set; }

    public int? CategoryId { get; private set; }
    public int? CityId { get; private set; }

    [MaxLength(MaxTitleLength)]
    public string? Title { get; private set; }

    [MaxLength(MaxDescriptionLength)]
    public string? Description { get; private set; }

    [Required, MaxLength(MaxFoodTypeLength)]
    public string FoodType { get; private set; } = string.Empty;

    [Required, MaxLength(MaxQuantityLength)]
    public string QuantityDescription { get; private set; } = string.Empty;

    [Column(TypeName = "decimal(10,2)")]
    public decimal? WeightKg { get; private set; }


    [Column(TypeName = "decimal(10,2)")]
    public decimal? Quantity { get; private set; } = decimal.Zero;

    [Required]
    public DateTime ExpiryDate { get; private set; }

    [Required, MaxLength(MaxPickupLocationLength)]
    public string PickupLocation { get; private set; } = string.Empty;
    [Required, MaxLength(500)]

    public string? ImageUrl { get; private set; }

    [Required]
    [Column(TypeName = "tinyint")]
    public DonationStatus Status { get; private set; }

    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? DeliveredAtUtc { get; private set; }
    public DateTime? CancelledAtUtc { get; private set; }
    public DateTime? ExpiredAtUtc { get; private set; }

    [MaxLength(MaxCancellationReasonLength)]
    public string? CancellationReason { get; private set; }

    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    [Timestamp]
    public byte[] RowVersion { get; private set; } = Array.Empty<byte>();

    [ForeignKey(nameof(DonorId))]
    public Account Donor { get; private set; } = null!;

    [ForeignKey(nameof(CharityId))]
    public Account? Charity { get; private set; }

    [ForeignKey(nameof(VolunteerId))]
    public Account? Volunteer { get; private set; }

    [ForeignKey(nameof(CategoryId))]
    public Category? Category { get; private set; }

    [ForeignKey(nameof(CityId))]
    public City? City { get; private set; }

    private readonly List<DeliveryMission> _deliveryMissions = new();
    public IReadOnlyCollection<DeliveryMission> DeliveryMissions => _deliveryMissions;

    private Donation() { }
}