using EcoBridge.Domain.Contracts;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Exceptions;
using EcoBridge.Domain.Models.Accounts;
using EcoBridge.Domain.Models.Common;

namespace EcoBridge.Domain.Models.Donation;

public partial class Donation : AggregateRoot, ISoftDeletable
{


    public Guid DonorId { get; private set; }
    public Guid? CharityId { get; private set; }
    public Guid? VolunteerId { get; private set; }
    public int? CategoryId { get; private set; }
    public int CityId { get; private set; }

    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string FoodType { get; private set; } = string.Empty;
    public string QuantityDescription { get; private set; } = string.Empty;
    public decimal? WeightKg { get; private set; }
    public DateTime ExpiryDate { get; private set; }
    public string PickupLocation { get; private set; } = string.Empty;
    public string? ImageUrl { get; private set; }

    public DonationStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? CancelledAtUtc { get; private set; }
    public DateTime? ExpiredAtUtc { get; private set; }
    public string? CancellationReason { get; private set; }

    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public byte[] RowVersion { get; private set; } = Array.Empty<byte>();

    // Navigation Properties
    public Account Donor { get; private set; } = null!;
    public Account? Charity { get; private set; }
    public Account? Volunteer { get; private set; }
    public Category? Category { get; private set; }
    public City City { get; private set; } = null!;

    private Donation() { }

    public static Donation Create(
        Guid donorId,
        string foodType,
        string quantity,
        DateTime expiryDate,
        string pickupLocation,
        int cityId,
        string title ,
        string description ,
        int? categoryId = null,
        decimal? weight = null,
        string? imageUrl = null)
    {
        if (donorId == Guid.Empty)
            throw new DomainException.ValidationException(AppMessageCodes.DONOR_ID_REQUIRED);

        if (!string.IsNullOrEmpty(imageUrl) && imageUrl.Length > 500)
            throw new DomainException.ValidationException(AppMessageCodes.IMAGE_PATH_TOO_LONG);

        ValidateDetails(foodType, quantity, expiryDate, pickupLocation, categoryId, title, description, weight, cityId);

        return new Donation
        {
            Id = Guid.NewGuid(),
            DonorId = donorId,
            FoodType = foodType.Trim(),
            QuantityDescription = quantity.Trim(),
            ExpiryDate = expiryDate,
            PickupLocation = pickupLocation.Trim(),
            CategoryId = categoryId,
            CityId = cityId,
            Title = Normalize(title),
            Description = Normalize(description),
            WeightKg = weight,
            ImageUrl = Normalize(imageUrl),
            Status = DonationStatus.Available,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdateDetails(
        string title,
        string description,
        string foodType,
        string quantity,
        decimal? weight,
        DateTime expiryDate,
        string pickupLocation,
        int categoryId,
        int cityId,
        string? imageUrl = null)
    {
        EnsureNotDeleted();
        EnsureNotExpired();

        if (Status != DonationStatus.Available)
            throw new DomainException.ConflictException(AppMessageCodes.DONATION_NOT_EDITABLE_IN_CURRENT_STATUS);

        if (!string.IsNullOrEmpty(imageUrl) && imageUrl.Length > 500)
            throw new DomainException.ValidationException(AppMessageCodes.IMAGE_PATH_TOO_LONG);

        ValidateDetails(foodType, quantity, expiryDate, pickupLocation, categoryId, title, description, weight, cityId);

        Title = Normalize(title) ?? Title;
        Description = Normalize(description) ?? Description;
        FoodType = foodType.Trim();
        QuantityDescription = quantity.Trim();
        WeightKg = weight;
        ExpiryDate = expiryDate;
        PickupLocation = pickupLocation.Trim();
        CategoryId = categoryId;
        CityId = cityId;
        ImageUrl = Normalize(imageUrl);
    }
}