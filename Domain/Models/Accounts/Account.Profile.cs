using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Exceptions;

namespace EcoBridge.Domain.Models.Accounts;

public partial class Account
{
    // ─────────────────────────────────────────────────────────────
    // Profile
    // ─────────────────────────────────────────────────────────────
    public void UpdateProfile(string fullName, string phone)
    {
        EnsureNotDeleted();

        if (string.IsNullOrWhiteSpace(fullName))
            throw new DomainException.ValidationException(AppMessageCodes.FULL_NAME_REQUIRED);

        if (string.IsNullOrWhiteSpace(phone))
            throw new DomainException.ValidationException(AppMessageCodes.PHONE_REQUIRED);
        phone = phone.Trim();
        if (phone.Length > 20)
            throw new DomainException.ValidationException(AppMessageCodes.PHONE_TOO_LONG);
        fullName = fullName.Trim();
        if (fullName.Length > 150)
            throw new DomainException.ValidationException(AppMessageCodes.FULL_NAME_TOO_LONG);
        FullName = fullName.Trim();
        PhoneNumber = phone.Trim();
    }

    public void SetLocation(int? cityId, string? addressDetails)
    {
        EnsureNotDeleted();
        SetLocationInternal(cityId, addressDetails);
    }

    private void SetLocationInternal(int? cityId, string? addressDetails)
    {
        if (cityId is <= 0)
            throw new DomainException.ValidationException(AppMessageCodes.INVALID_CITY_ID);

        CityId = cityId;
        AddressDetails = string.IsNullOrWhiteSpace(addressDetails)
            ? null
            : addressDetails.Trim();
    }

    public void UpdateProfileImage(string imagePath)
    {
        EnsureNotDeleted();

        if (string.IsNullOrWhiteSpace(imagePath))
            throw new DomainException.ValidationException(AppMessageCodes.IMAGE_PATH_REQUIRED);
        if (imagePath.Length > 500)
            throw new DomainException.ValidationException(AppMessageCodes.IMAGE_PATH_TOO_LONG);
        ImagePath = imagePath.Trim();
    }

    public void RemoveProfileImage()
    {
        EnsureNotDeleted();
        ImagePath = null;
    }
    public void ChangeEmail(string newEmail)
    {
        EnsureNotDeleted();

        if (string.IsNullOrWhiteSpace(newEmail))
            throw new DomainException.ValidationException(AppMessageCodes.EMAIL_REQUIRED);

        newEmail = newEmail.Trim();
        if (newEmail.Length > 256)
            throw new DomainException.ValidationException(AppMessageCodes.EMAIL_TOO_LONG);

        if (!newEmail.Contains('@'))
            throw new DomainException.ValidationException(AppMessageCodes.INVALID_EMAIL_FORMAT);

        Email = newEmail;
        NormalizedEmail = newEmail.ToUpperInvariant();
    }

    public void ChangePhoneNumber(string newPhoneNumber)
    {
        EnsureNotDeleted();

        if (string.IsNullOrWhiteSpace(newPhoneNumber))
            throw new DomainException.ValidationException(AppMessageCodes.PHONE_REQUIRED);

        newPhoneNumber = newPhoneNumber.Trim();
        if (newPhoneNumber.Length > 20)
            throw new DomainException.ValidationException(AppMessageCodes.PHONE_TOO_LONG);
    
        if (!System.Text.RegularExpressions.Regex.IsMatch(newPhoneNumber, @"^\+?[0-9]{1,3}[-.\s]?(\([0-9]{1,4}\))?[0-9]{1,4}[-.\s]?[0-9]{1,9}$"))
            throw new DomainException.ValidationException(AppMessageCodes.INVALID_PHONE_FORMAT);

        PhoneNumber = newPhoneNumber;
    }
}