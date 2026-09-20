using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Exceptions;
using EcoBridge.Domain.Models.Mission;

namespace EcoBridge.Domain.Models.Donation;

public partial class Donation
{
    // ─────────────────────────────────────────────────────────────
    // Helpers / Guards
    // ─────────────────────────────────────────────────────────────
    private DeliveryMission RequireActiveMission()
    {
        var mission = GetActiveMission();
        if (mission is null)
            throw new DomainException.ConflictException(
                AppMessageCodes.DONATION_NO_ACTIVE_MISSION);
        return mission;
    }

    private void EnsureInTransit()
    {
        if (Status != Enums.DonationStatus.InTransit)
            throw new DomainException.ConflictException(
                AppMessageCodes.DONATION_NOT_IN_TRANSIT);
    }

    private void EnsureNotDeleted()
    {
        if (IsDeleted)
            throw new DomainException.ConflictException(
                AppMessageCodes.DONATION_DELETED);
    }

    private void EnsureNotExpired()
    {
        if (IsExpired())
            throw new DomainException.ValidationException(
                AppMessageCodes.DONATION_EXPIRED);
    }

    private static void ValidateDetails(
        string foodType,
        string quantity,
        DateTime expiryDate,
        string pickupLocation,
        int? categoryId,
        string? title,
        string? description,
        decimal? weight,
        int? cityId)
    {
        if (string.IsNullOrWhiteSpace(foodType))
            throw new DomainException.ValidationException(AppMessageCodes.FOOD_TYPE_REQUIRED);

        if (string.IsNullOrWhiteSpace(quantity))
            throw new DomainException.ValidationException(AppMessageCodes.QUANTITY_REQUIRED);

        if (string.IsNullOrWhiteSpace(pickupLocation))
            throw new DomainException.ValidationException(AppMessageCodes.PICKUP_LOCATION_REQUIRED);

        if (expiryDate <= DateTime.UtcNow)
            throw new DomainException.ValidationException(AppMessageCodes.DONATION_EXPIRED);

        if (foodType.Trim().Length > MaxFoodTypeLength)
            throw new DomainException.ValidationException(AppMessageCodes.FOOD_TYPE_TOO_LONG);

        if (quantity.Trim().Length > MaxQuantityLength)
            throw new DomainException.ValidationException(AppMessageCodes.QUANTITY_TOO_LONG);

        if (pickupLocation.Trim().Length > MaxPickupLocationLength)
            throw new DomainException.ValidationException(AppMessageCodes.PICKUP_LOCATION_TOO_LONG);

        if (Normalize(title)?.Length > MaxTitleLength)
            throw new DomainException.ValidationException(AppMessageCodes.TITLE_TOO_LONG);

        if (Normalize(description)?.Length > MaxDescriptionLength)
            throw new DomainException.ValidationException(AppMessageCodes.DESCRIPTION_TOO_LONG);

        if (categoryId is <= 0)
            throw new DomainException.ValidationException(AppMessageCodes.INVALID_CATEGORY_ID);

        if (cityId is <= 0)
            throw new DomainException.ValidationException(AppMessageCodes.INVALID_CITY_ID);

        if (weight is <= 0)
            throw new DomainException.ValidationException(AppMessageCodes.INVALID_WEIGHT);

        if (weight > MaxWeightKg)
            throw new DomainException.ValidationException(AppMessageCodes.WEIGHT_EXCEEDS_LIMIT);
    }

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}