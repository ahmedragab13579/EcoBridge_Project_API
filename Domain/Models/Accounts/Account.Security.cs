using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Exceptions;

namespace EcoBridge.Domain.Models.Accounts;

public partial class Account
{
    // ─────────────────────────────────────────────────────────────
    // Security
    // ─────────────────────────────────────────────────────────────
    public void ChangePassword(string newPasswordHash)
    {
        EnsureNotDeleted();

        if (string.IsNullOrWhiteSpace(newPasswordHash))
            throw new DomainException.ValidationException(AppMessageCodes.PASSWORD_HASH_REQUIRED);

        PasswordHash = newPasswordHash;
        SecurityStamp = Guid.NewGuid().ToString();
    }
}