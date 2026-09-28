using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using EcoBridge.Domain.Constants;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Exceptions;

namespace EcoBridge.Domain.Models;

public class Category
{
    public int Id { get; private set; }

    public string NameAr { get; private set; } = string.Empty;


    public string NameEn { get; private set; } = string.Empty;

    private List<Donation.Donation> _donations = new List<Donation.Donation>();
    public IReadOnlyCollection<Donation.Donation> Donations =>  _donations;

    // ─────────────────────────────────────────────────────────────
    // EF Core constructor
    // ─────────────────────────────────────────────────────────────
    private Category() { }

    // ─────────────────────────────────────────────────────────────
    // Factory Method — the only valid creation path
    // ─────────────────────────────────────────────────────────────
    public static Category Create(string nameAr, string nameEn)
    {
        ValidateAndNormalizeNames(nameAr, nameEn, out var trimmedAr, out var trimmedEn);

        return new Category
        {
            NameAr = trimmedAr,
            NameEn = trimmedEn
        };
    }

    // ─────────────────────────────────────────────────────────────
    // Behaviors
    // ─────────────────────────────────────────────────────────────
    public void Rename(string nameAr, string nameEn)
    {
        ValidateAndNormalizeNames(nameAr, nameEn, out var trimmedAr, out var trimmedEn);

        NameAr = trimmedAr;
        NameEn = trimmedEn;
    }

    // ─────────────────────────────────────────────────────────────
    // Domain queries (pure, no side effects)
    // ─────────────────────────────────────────────────────────────
    public bool IsInUse() => Donations.Count > 0;

    // ─────────────────────────────────────────────────────────────
    // Invariants
    // ─────────────────────────────────────────────────────────────
    private static void ValidateAndNormalizeNames(
        string nameAr,
        string nameEn,
        out string trimmedAr,
        out string trimmedEn)
    {
        if (string.IsNullOrWhiteSpace(nameAr))
            throw new DomainException.ValidationException(AppMessageCodes.CATEGORY_NAME_AR_REQUIRED);

        if (string.IsNullOrWhiteSpace(nameEn))
            throw new DomainException.ValidationException(AppMessageCodes.CATEGORY_NAME_EN_REQUIRED);

        trimmedAr = nameAr.Trim();
        trimmedEn = nameEn.Trim();

        if (trimmedAr.Length >DomainConstants.Name.MaxNameLength)
            throw new DomainException.ValidationException(AppMessageCodes.CATEGORY_NAME_AR_TOO_LONG);

        if (trimmedEn.Length > DomainConstants.Name.MaxNameLength)
            throw new DomainException.ValidationException(AppMessageCodes.CATEGORY_NAME_EN_TOO_LONG);
    }
}