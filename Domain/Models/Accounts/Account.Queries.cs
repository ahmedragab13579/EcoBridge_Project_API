using EcoBridge.Domain.Enums;

namespace EcoBridge.Domain.Models.Accounts;

public partial class Account
{
    // ─────────────────────────────────────────────────────────────
    // Domain queries
    // ─────────────────────────────────────────────────────────────
    public bool IsAdmin() => RoleId == UserRole.Admin;
    public bool IsDonor() => RoleId == UserRole.Donor;
    public bool IsCharity() => RoleId == UserRole.Charity;
    public bool IsVolunteer() => RoleId == UserRole.Volunteer;

    public bool IsCompany() => UserType == UserType.Company;
    public bool IsIndividual() => UserType == UserType.Individual;

    public bool CanReceiveDonations() => IsActive && !IsDeleted && IsCharity();
    public bool CanHandleDeliveries() => IsActive && !IsDeleted && IsVolunteer() && IsAvailable;
    public bool CanDonate() => IsActive && !IsDeleted && IsDonor();
}