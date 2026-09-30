using EcoBridge.Domain.Enums;

namespace EcoBridge.Domain.Models.Accounts;

public partial class Account
{
    // ─────────────────────────────────────────────────────────────
    // Domain queries
    // ─────────────────────────────────────────────────────────────
    public bool IsAdmin() => Role == UserRole.Admin;
    public bool IsDonor() => Role == UserRole.Donor;
    public bool IsCharity() => Role == UserRole.Charity;
    public bool IsVolunteer() => Role == UserRole.Volunteer;

    public bool IsCompany() => UserType == UserType.Company;
    public bool IsIndividual() => UserType == UserType.Individual;

    public bool CanReceiveDonations() => IsActive && !IsDeleted && IsCharity();
    public bool CanHandleDeliveries() => IsActive && !IsDeleted && IsVolunteer() && IsAvailable;
    public bool CanDonate() => IsActive && !IsDeleted && IsDonor();
}