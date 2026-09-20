using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Exceptions;

namespace EcoBridge.Domain.Models.Donation;

public partial class Donation
{
    // ─────────────────────────────────────────────────────────────
    // Factory
    // ─────────────────────────────────────────────────────────────
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
        if (donorId == Guid.Empty)
            throw new DomainException.ValidationException(AppMessageCodes.DONOR_ID_REQUIRED);
        if (!string.IsNullOrEmpty(imageUrl) && imageUrl.Length > 500)
            throw new DomainException.ValidationException(AppMessageCodes.IMAGE_PATH_TOO_LONG);

        ValidateDetails(
            foodType, quantity, expiryDate, pickupLocation,
            categoryId, title, description, weight, cityId);

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
}