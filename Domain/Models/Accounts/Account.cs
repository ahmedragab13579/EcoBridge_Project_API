using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EcoBridge.Domain.Constants;
using EcoBridge.Domain.Contracts;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Models.Mission;
using Microsoft.AspNetCore.Identity;

namespace EcoBridge.Domain.Models.Accounts;

public partial class Account : IdentityUser<Guid> ,ISoftDeletable
{



    public string FullName { get; private set; } = string.Empty;


    public UserRole RoleId { get; private set; }

 
    public UserType UserType { get; private set; } = UserType.Individual;

    public int TotalWalletPoints { get; private set; } = DomainConstants.Score.InitialWalletPoints;
    public int TrustScore { get; private set; } = DomainConstants.Score.InitialTrustScore;
    public int? CityId { get; private set; }

    public string? AddressDetails { get; private set; }

    public City? City { get; private set; }


    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    public string? ImagePath { get; private set; }

    public bool IsActive { get; private set; } = true;

    public bool IsDeleted { get; private set; } = false;
    public DateTime? DeletedAt { get; private set; }

    public string? OrganizationName { get; private set; }

    public string? RegistrationNumber { get; private set; }

    public string? VehicleDetails { get; private set; }

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
    private readonly List<TrustCreditLog> _trustCreditLogs = new();
    public IReadOnlyCollection<TrustCreditLog> trustCreditLogs => _trustCreditLogs;

    private Account() 
    {
        
    }
}