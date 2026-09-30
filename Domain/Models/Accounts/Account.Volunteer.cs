using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Exceptions;

namespace EcoBridge.Domain.Models.Accounts;

public partial class Account
{
    // ─────────────────────────────────────────────────────────────
    // Volunteer behaviors
    // ─────────────────────────────────────────────────────────────
    public void SetVehicleInfo(string? vehicleType, string? vehicleDetails)
    {
        if (Role != UserRole.Volunteer)
            throw new DomainException.ForbiddenException(AppMessageCodes.VOLUNTEER_ONLY_OPERATION);

        EnsureNotDeleted();
        SetVehicleInfoInternal(vehicleType, vehicleDetails);
    }

    private void SetVehicleInfoInternal(string? vehicleType, string? vehicleDetails)
    {
        VehicleType = string.IsNullOrWhiteSpace(vehicleType) ? null : vehicleType.Trim();
        VehicleDetails = string.IsNullOrWhiteSpace(vehicleDetails) ? null : vehicleDetails.Trim();
    }

    public void SetAvailability(bool isAvailable)
    {
        if (Role != UserRole.Volunteer)
            throw new DomainException.ForbiddenException(AppMessageCodes.VOLUNTEER_ONLY_OPERATION);

        EnsureNotDeleted();

        if (isAvailable && !IsActive)
            throw new DomainException.ConflictException(AppMessageCodes.ACCOUNT_DEACTIVATED);

        IsAvailable = isAvailable;
    }
}