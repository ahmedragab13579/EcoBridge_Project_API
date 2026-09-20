using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Exceptions;

namespace EcoBridge.Domain.Models.Accounts;

public partial class Account
{
    // ─────────────────────────────────────────────────────────────
    // Charity behaviors
    // ─────────────────────────────────────────────────────────────
    public void SetOrganizationInfo(string? organizationName, string? registrationNumber)
    {
        if (RoleId != UserRole.Charity)
            throw new DomainException.ForbiddenException(AppMessageCodes.CHARITY_ONLY_OPERATION);

        EnsureNotDeleted();
        SetOrganizationInfoInternal(organizationName, registrationNumber);
    }

    private void SetOrganizationInfoInternal(string? organizationName, string? registrationNumber)
    {
        OrganizationName = string.IsNullOrWhiteSpace(organizationName) ? null : organizationName.Trim();
        RegistrationNumber = string.IsNullOrWhiteSpace(registrationNumber) ? null : registrationNumber.Trim();
    }
}