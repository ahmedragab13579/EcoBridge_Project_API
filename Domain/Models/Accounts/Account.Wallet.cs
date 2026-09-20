using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Exceptions;

namespace EcoBridge.Domain.Models.Accounts;

public partial class Account
{
    // ─────────────────────────────────────────────────────────────
    // Wallet
    // ─────────────────────────────────────────────────────────────
    public void AddPoints(int points)
    {
        if (points <= 0)
            throw new DomainException.ValidationException(AppMessageCodes.INVALID_POINTS_AMOUNT);

        if ( MaxWalletPoints - TotalWalletPoints < points)
            throw new DomainException.ValidationException(AppMessageCodes.WALLET_POINTS_LIMIT_EXCEEDED);

        TotalWalletPoints += points;
    }

    public void DeductPoints(int points)
    {
        if (points <= 0)
            throw new DomainException.ValidationException(AppMessageCodes.INVALID_POINTS_AMOUNT);

        if (points > TotalWalletPoints)
            throw new DomainException.ValidationException(AppMessageCodes.INSUFFICIENT_POINTS);

        TotalWalletPoints -= points;
    }

    public bool TryDeductPoints(int points, out int remainingPoints)
    {
        if (points <= 0)
            throw new DomainException.ValidationException(AppMessageCodes.INVALID_POINTS_AMOUNT);

        if (points > TotalWalletPoints)
        {
            remainingPoints = TotalWalletPoints;
            return false;
        }

        TotalWalletPoints -= points;
        remainingPoints = TotalWalletPoints;
        return true;
    }

    public void ResetWalletPoints()
    {
        EnsureNotDeleted();
        TotalWalletPoints = InitialWalletPoints;
    }
}