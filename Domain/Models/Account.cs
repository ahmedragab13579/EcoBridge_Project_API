using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EcoBridge.Domain.Contracts;
using EcoBridge.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace EcoBridge.Domain.Models;

public class Account : IdentityUser<Guid>, ISoftDeletable
{
    [Required]
    [MaxLength(150)]
    public string FullName { get; protected set; } = string.Empty;

    [NotMapped]
    public string Phone
    {
        get => PhoneNumber ?? string.Empty;
        protected set => PhoneNumber = value;
    }

    [Required]
    [Column(TypeName = "tinyint")]
    public UserRole RoleId { get; protected set; }

    [Required]
    [Column(TypeName = "tinyint")]
    public UserType UserType { get; protected set; } = UserType.Individual;

    public int TotalWalletPoints { get; protected set; } = 0;

    public int TrustScore { get; protected set; } = 100;

    public int? CityId { get; protected set; }

    [MaxLength(500)]
    public string? AddressDetails { get; protected set; }

    [ForeignKey(nameof(CityId))]
    public City? City { get; set; }

    [Required]
    [Column(TypeName = "datetime2")]
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;

    [MaxLength(500)]
    public string? ImagePath { get; protected set; }

    [Required]
    public bool IsActive { get; protected set; } = true;

    public bool IsDeleted { get; set; } = false;

    [Column(TypeName = "datetime2")]
    public DateTime? DeletedAt { get; set; }

    public Donor? Donor { get; set; }
    public Charity? Charity { get; set; }
    public Volunteer? Volunteer { get; set; }
    public Admin? Admin { get; set; }
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public ICollection<PointTransaction> PointTransactions { get; set; } = new List<PointTransaction>();
    public ICollection<Review> ReviewsGiven { get; set; } = new List<Review>();
    public ICollection<Review> ReviewsReceived { get; set; } = new List<Review>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public ICollection<PenaltyLog> PenaltyLogs { get; set; } = new List<PenaltyLog>();

    public Account() { }

    public static Account Create(
        string fullName,
        string email,
        string phone,
        string passwordHash,
        UserRole roleId,
        UserType userType = UserType.Individual,
        int? cityId = null,
        string? addressDetails = null)
    {
        return new Account
        {
            Id = Guid.NewGuid(),
            FullName = fullName,
            Email = email,
            UserName = email,
            NormalizedEmail = email.ToUpperInvariant(),
            NormalizedUserName = email.ToUpperInvariant(),
            PhoneNumber = phone,
            PasswordHash = passwordHash,
            RoleId = roleId,
            UserType = userType,
            CityId = cityId,
            AddressDetails = addressDetails?.Trim(),
            TotalWalletPoints = 0,
            TrustScore = 100,
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            IsDeleted = false,
            DeletedAt = null,
            SecurityStamp = Guid.NewGuid().ToString()
        };
    }

    public void UpdateProfile(string fullName, string phone, int? cityId, string? addressDetails = null)
    {
        if (!string.IsNullOrWhiteSpace(fullName)) FullName = fullName;
        if (!string.IsNullOrWhiteSpace(phone)) PhoneNumber = phone;
        if (cityId.HasValue) CityId = cityId;
        if (addressDetails != null) AddressDetails = string.IsNullOrWhiteSpace(addressDetails) ? null : addressDetails.Trim();
    }

    public void UpdateProfileImage(string imagePath)
    {
        if (!string.IsNullOrWhiteSpace(imagePath)) ImagePath = imagePath;
    }

    public void ChangePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
    }

    public void AddPoints(int points)
    {
        if (points > 0)
        {
            TotalWalletPoints += points;
        }
    }

    public void DeductPoints(int points)
    {
        if (points > 0)
        {
            TotalWalletPoints = Math.Max(0, TotalWalletPoints - points);
        }
    }

    public void AdjustTrustScore(int delta)
    {
        TrustScore = Math.Clamp(TrustScore + delta, 0, 100);
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void SoftDelete()
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        IsActive = false;
    }

    public void Restore()
    {
        IsDeleted = false;
        DeletedAt = null;
        IsActive = true;
    }
}
