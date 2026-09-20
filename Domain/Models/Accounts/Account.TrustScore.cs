using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Exceptions;

namespace EcoBridge.Domain.Models.Accounts;

public partial class Account
{
    // ─────────────────────────────────────────────────────────────
    // Trust score
    // ─────────────────────────────────────────────────────────────
    public void IncreaseTrustScore(int amount)
    {
                EnsureNotDeleted();

        if (amount <= 0)
            throw new DomainException.ValidationException(AppMessageCodes.INVALID_TRUST_SCORE_AMOUNT);

        if (MaxTrustScore - TrustScore < amount)
            throw new DomainException.ValidationException(AppMessageCodes.TRUST_SCORE_LIMIT_EXCEEDED);  

        TrustScore = Math.Min(MaxTrustScore, TrustScore + amount);
    }

   public void DecreaseTrustScore(int amount)
{
    if (amount <= 0)
        throw new DomainException.ValidationException(AppMessageCodes.INVALID_TRUST_SCORE_AMOUNT);

    EnsureNotDeleted(); 

    if (TrustScore - MinTrustScore < amount)
        throw new DomainException.ValidationException(AppMessageCodes.TRUST_SCORE_LIMIT_EXCEEDED);

    TrustScore -= amount;
}
    public void ResetTrustScore()
    {
        EnsureNotDeleted();
        TrustScore = InitialTrustScore;
    }
}