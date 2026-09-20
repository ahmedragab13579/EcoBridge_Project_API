using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Exceptions;

namespace EcoBridge.Domain.Models.Accounts;

public partial class Account
{
    // ─────────────────────────────────────────────────────────────
    // Invariants
    // ─────────────────────────────────────────────────────────────
    private void EnsureNotDeleted()
    {
        if (IsDeleted)
            throw new DomainException.ConflictException(AppMessageCodes.ACCOUNT_DELETED);
    }

    private static void EnsureRoleTypeCompatibility(UserRole role, UserType type)
    {
        if (role == UserRole.Admin && type == UserType.Company)
            throw new DomainException.ValidationException(AppMessageCodes.ADMIN_CANNOT_BE_COMPANY);

        if (role == UserRole.Charity && type != UserType.Company)
            throw new DomainException.ValidationException(AppMessageCodes.CHARITY_MUST_BE_COMPANY);
    }
}