using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Exceptions;

namespace EcoBridge.Domain.Models.Donation;

public partial class Donation
{
    // ─────────────────────────────────────────────────────────────
    // Editing — only while Available
    // ─────────────────────────────────────────────────────────────
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
        EnsureNotDeleted();
        EnsureNotExpired();

        if (Status != DonationStatus.Available)
            throw new DomainException.ConflictException(
                AppMessageCodes.DONATION_NOT_EDITABLE_IN_CURRENT_STATUS);
 if (!string.IsNullOrEmpty(imageUrl) && imageUrl.Length > 500)
            throw new DomainException.ValidationException(AppMessageCodes.IMAGE_PATH_TOO_LONG);

        ValidateDetails(
            foodType, quantity, expiryDate, pickupLocation,
            categoryId, title, description, weight, cityId);

        Title = Normalize(title);
        Description = Normalize(description);
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