using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Exceptions;

namespace EcoBridge.Domain.Models.Accounts;

public partial class Account
{
    // ─────────────────────────────────────────────────────────────
    // Lifecycle
    // ─────────────────────────────────────────────────────────────
    public void Activate()
    {
        if (IsDeleted)
            throw new DomainException.ConflictException(AppMessageCodes.ACCOUNT_DELETED);

        if (IsActive) return;

        IsActive = true;
    }

    public void Deactivate()
    {
        if (IsDeleted)
            throw new DomainException.ConflictException(AppMessageCodes.ACCOUNT_DELETED);

        if (!IsActive) return;

        IsActive = false;
        IsAvailable = false;
    }

    public void SoftDelete()
    {
        if (IsDeleted) return;

        IsDeleted = true;
        IsActive = false;
        IsAvailable = false;
        DeletedAt = DateTime.UtcNow;
    }

    public void Restore()
    {
        if (!IsDeleted) return;

        IsDeleted = false;
        DeletedAt = null;
        IsActive = true;
    }
}