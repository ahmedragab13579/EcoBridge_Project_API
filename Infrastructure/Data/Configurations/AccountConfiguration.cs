using EcoBridge.Domain.Models.Accounts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcoBridge.Infrastructure.Data.Configurations;

public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        // ═════════════════════════════════════════════════════════
        //  Table
        // ═════════════════════════════════════════════════════════
        builder.ToTable("Accounts", t =>
        {
            t.HasCheckConstraint(
                "CK_Accounts_TrustScore_Range",
                "[TrustScore] >= 0 AND [TrustScore] <= 100");

            t.HasCheckConstraint(
                "CK_Accounts_WalletPoints_Range",
                "[TotalWalletPoints] >= 0 AND [TotalWalletPoints] <= 1000000");
        });

        // ═════════════════════════════════════════════════════════
        //  Scalar properties
        // ═════════════════════════════════════════════════════════
        builder.Property(a => a.FullName)
            .HasMaxLength(150)
            .IsRequired();


        builder.Property(a => a.RoleId)
            .HasConversion<string>()
            .HasColumnType("tinyint")
            .IsRequired();

        builder.Property(a => a.UserType)
            .HasConversion<string>()
            .HasColumnType("tinyint")
            .IsRequired();

        builder.Property(a => a.TotalWalletPoints)
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(a => a.TrustScore)
            .HasDefaultValue(100)
            .IsRequired();

        builder.Property(a => a.CityId)
            .IsRequired(false);

        builder.Property(a => a.AddressDetails)
            .HasMaxLength(500);

        builder.Property(a => a.CreatedAt)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(a => a.ImagePath)
            .HasMaxLength(500);

        builder.Property(a => a.IsActive)
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(a => a.IsDeleted)
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(a => a.DeletedAt)
            .HasColumnType("datetime2");

        // ── Charity-specific ────────────────────────────────────
        builder.Property(a => a.OrganizationName)
            .HasMaxLength(150);

        builder.Property(a => a.RegistrationNumber)
            .HasMaxLength(50);

        // ── Volunteer-specific ──────────────────────────────────
        builder.Property(a => a.VehicleDetails)
            .HasMaxLength(100);

        builder.Property(a => a.VehicleType)
            .HasMaxLength(50);

        builder.Property(a => a.IsAvailable)
            .HasDefaultValue(false)
            .IsRequired();

        // ═════════════════════════════════════════════════════════
        //  Backing fields — collection navigations
        // ═════════════════════════════════════════════════════════
        builder.Metadata
            .FindNavigation(nameof(Account.DonationsMade))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.Metadata
            .FindNavigation(nameof(Account.DonationsReceived))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.Metadata
            .FindNavigation(nameof(Account.DonationsHandled))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.Metadata
            .FindNavigation(nameof(Account.DeliveryMissions))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.Metadata
            .FindNavigation(nameof(Account.RefreshTokens))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.Metadata
            .FindNavigation(nameof(Account.PointTransactions))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.Metadata
            .FindNavigation(nameof(Account.ReviewsGiven))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.Metadata
            .FindNavigation(nameof(Account.ReviewsReceived))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.Metadata
            .FindNavigation(nameof(Account.Notifications))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.Metadata
            .FindNavigation(nameof(Account.PenaltyLogs))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        // ═════════════════════════════════════════════════════════
        //  Relationships
        // ═════════════════════════════════════════════════════════

        // ── Account → City (Restrict) ───────────────────────────
        builder.HasOne(a => a.City)
            .WithMany()
            .HasForeignKey(a => a.CityId)
            .OnDelete(DeleteBehavior.Restrict);

        // ═════════════════════════════════════════════════════════
        //  Indexes
        // ═════════════════════════════════════════════════════════

        // Email indexes already provided by Identity (unique).

        // Phone number is NOT unique (per your decision).
        builder.HasIndex(a => a.PhoneNumber)
            .HasDatabaseName("IX_Accounts_PhoneNumber");
        builder.HasIndex(u => new { u.Email, u.IsDeleted })
            .IsUnique();


        builder.HasIndex(a => a.RoleId)
            .HasDatabaseName("IX_Accounts_RoleId");

        builder.HasIndex(a => a.CityId)
            .HasDatabaseName("IX_Accounts_CityId");

        // Composite index for the most common query:
        // "get active, non-deleted accounts".
        builder.HasIndex(a => new { a.IsActive, a.IsDeleted })
            .HasDatabaseName("IX_Accounts_IsActive_IsDeleted");

        // Volunteer discovery: search for available volunteers in a city.
        builder.HasIndex(a => new { a.RoleId, a.IsAvailable, a.CityId })
            .HasDatabaseName("IX_Accounts_Role_Available_City");
    }
}