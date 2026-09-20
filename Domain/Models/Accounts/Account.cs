using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EcoBridge.Domain.Contracts;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Models.Mission;
using Microsoft.AspNetCore.Identity;

namespace EcoBridge.Domain.Models.Accounts;

public partial class Account : IdentityUser<Guid>, ISoftDeletable
{
    private const int MaxTrustScore = 100;
    private const int MinTrustScore = 0;
    private const int InitialTrustScore = 100;
    private const int InitialWalletPoints = 0;
    private const int MaxWalletPoints = 1_000_000;

    [Required]
    [MaxLength(150)]
    public string FullName { get; private set; } = string.Empty;

    [Required]
    [Column(TypeName = "tinyint")]
    public UserRole RoleId { get; private set; }

    [Required]
    [Column(TypeName = "tinyint")]
    public UserType UserType { get; private set; } = UserType.Individual;

    public int TotalWalletPoints { get; private set; } = InitialWalletPoints;
    public int TrustScore { get; private set; } = InitialTrustScore;
    public int? CityId { get; private set; }

    [MaxLength(500)]
    public string? AddressDetails { get; private set; }

    [ForeignKey(nameof(CityId))]
    public City? City { get; private set; }

    [Required]
    [Column(TypeName = "datetime2")]
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    [MaxLength(500)]
    public string? ImagePath { get; private set; }

    [Required]
    public bool IsActive { get; private set; } = true;

    public bool IsDeleted { get; private set; } = false;
    public DateTime? DeletedAt { get; private set; }

    [MaxLength(150)]
    public string? OrganizationName { get; private set; }

    [MaxLength(50)]
    public string? RegistrationNumber { get; private set; }

    [MaxLength(100)]
    public string? VehicleDetails { get; private set; }

    [MaxLength(50)]
    public string? VehicleType { get; private set; }

    public bool IsAvailable { get; private set; }

    // --- Donations ---
    private readonly List<Donation.Donation> _donationsMade = new();
    public IReadOnlyCollection<Donation.Donation> DonationsMade => _donationsMade;

    private readonly List<Donation.Donation> _donationsReceived = new();
    public IReadOnlyCollection<Donation.Donation> DonationsReceived => _donationsReceived;

    private readonly List<Donation.Donation> _donationsHandled = new();
    public IReadOnlyCollection<Donation.Donation> DonationsHandled => _donationsHandled;

    // --- Delivery Missions ---
    private readonly List<DeliveryMission> _deliveryMissions = new();
    public IReadOnlyCollection<DeliveryMission> DeliveryMissions => _deliveryMissions;

    // --- Auth & Points ---
    private readonly List<RefreshToken> _refreshTokens = new();
    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens;

    private readonly List<PointTransaction> _pointTransactions = new();
    public IReadOnlyCollection<PointTransaction> PointTransactions => _pointTransactions;

    // --- Reviews ---
    private readonly List<Review> _reviewsGiven = new();
    public IReadOnlyCollection<Review> ReviewsGiven => _reviewsGiven;

    private readonly List<Review> _reviewsReceived = new();
    public IReadOnlyCollection<Review> ReviewsReceived => _reviewsReceived;

    // --- Notifications & Penalties ---
    private readonly List<Notification> _notifications = new();
    public IReadOnlyCollection<Notification> Notifications => _notifications;

    private readonly List<PenaltyLog> _penaltyLogs = new();
    public IReadOnlyCollection<PenaltyLog> PenaltyLogs => _penaltyLogs;

    private Account() 
    {
        
    }
}