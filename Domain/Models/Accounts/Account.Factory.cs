using EcoBridge.Domain.Constants;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Exceptions;

namespace EcoBridge.Domain.Models.Accounts;

public partial class Account
{
    public static Account Create(
        string fullName,
        string email,
        string phone,
        string passwordHash,
        UserRole role,
        UserType userType = UserType.Individual,
        int? cityId = null,
        string? addressDetails = null,
        string? organizationName = null,
        string? registrationNumber = null,
        string? vehicleType = null,
        string? vehicleDetails = null
        )
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new DomainException.ValidationException(AppMessageCodes.FULL_NAME_REQUIRED);

        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException.ValidationException(AppMessageCodes.EMAIL_REQUIRED);
        if (email.Length>100)
            throw new DomainException.ValidationException(AppMessageCodes.EMAIL_TOO_LONG);
        if (!email.Contains('@')||!email.Contains('.'))
            throw new DomainException.ValidationException(AppMessageCodes.INVALID_EMAIL_FORMAT);
        if (phone.Length > DomainConstants.Body.MaxPhoneLength)
            throw new DomainException.ValidationException(AppMessageCodes.PHONE_TOO_LONG);

        if (!System.Text.RegularExpressions.Regex.IsMatch(phone, @"^\+?[0-9]{1,3}[-.\s]?(\([0-9]{1,4}\))?[0-9]{1,4}[-.\s]?[0-9]{1,9}$"))
            throw new DomainException.ValidationException(AppMessageCodes.INVALID_PHONE_FORMAT);

        if (string.IsNullOrWhiteSpace(phone))
            throw new DomainException.ValidationException(AppMessageCodes.PHONE_REQUIRED);

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainException.ValidationException(AppMessageCodes.PASSWORD_HASH_REQUIRED);

        if (!Enum.IsDefined(role))
            throw new DomainException.ValidationException(AppMessageCodes.INVALID_ROLE);

        if (!Enum.IsDefined(userType))
            throw new DomainException.ValidationException(AppMessageCodes.INVALID_USER_TYPE);

        if(role == UserRole.Charity && (string.IsNullOrWhiteSpace(organizationName) || string.IsNullOrWhiteSpace(registrationNumber)))
            throw new DomainException.ValidationException(AppMessageCodes.ORGANIZATION_INFO_REQUIRED);

        EnsureRoleTypeCompatibility(role, userType);

        var account = new Account
        {
            Id = Guid.NewGuid(),
            FullName = fullName.Trim(),
            Email = email.Trim(),
            UserName = email.Trim(),
            NormalizedEmail = email.Trim().ToUpperInvariant(),
            NormalizedUserName = email.Trim().ToUpperInvariant(),
            PhoneNumber = phone.Trim(),
            PasswordHash = passwordHash,
            Role = role,
            UserType = userType,
            TotalWalletPoints = DomainConstants.Score.InitialWalletPoints,
            TrustScore = DomainConstants.Score.InitialTrustScore,
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            IsDeleted = false,
            IsAvailable = false,
            SecurityStamp = Guid.NewGuid().ToString()
        };

        account.SetLocationInternal(cityId, addressDetails);

        switch (role)
        {
            case UserRole.Charity:
                account.SetOrganizationInfoInternal(organizationName, registrationNumber);
                break;
            case UserRole.Volunteer:
                account.SetVehicleInfoInternal(vehicleType, vehicleDetails);
                account.IsAvailable = true;
                break;
            case UserRole.Donor:
            case UserRole.Admin:
                break;
        }

        return account;
    }
}