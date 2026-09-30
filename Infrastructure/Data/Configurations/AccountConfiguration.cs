using EcoBridge.Domain.Constants;
using EcoBridge.Domain.Models.Accounts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcoBridge.Infrastructure.Data.Configurations;

public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        // ═════════════════════════════════════════════════════════
        //  Table + Check Constraints
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
            .HasMaxLength(DomainConstants.Name.MaxNameLength)
            .IsRequired();


        builder.Property(a => a.RowVersion).IsRowVersion();

        builder.Property(a => a.Role)
            .HasConversion<string>()
            .HasMaxLength(DomainConstants.Name.MaxCodeLength)
            .IsRequired();

        builder.Property(a => a.UserType)
            .HasConversion<string>()
            .HasMaxLength(DomainConstants.Name.MaxCodeLength)
            .IsRequired();

        builder.Property(a => a.TotalWalletPoints)
            .HasDefaultValue(DomainConstants.Score.InitialWalletPoints)
            .IsRequired();

        builder.Property(a => a.TrustScore)
            .HasDefaultValue(DomainConstants.Score.MaxTrustScore)
            .IsRequired();

        builder.Property(a => a.CityId)
            .IsRequired(false);

        builder.Property(a => a.AddressDetails)
            .HasMaxLength(DomainConstants.Body.MaxLocationLength);

        builder.Property(a => a.CreatedAt)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(a => a.ImagePath)
            .HasMaxLength(DomainConstants.Body.MaxBodyLength);

        builder.Property(a => a.IsActive)
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(a => a.IsDeleted)
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(a => a.DeletedAt)
            .HasColumnType("datetime2");

        // ── Charity-specific ────────────────────────────────────
        builder.Property(a => a.OrganizationName).HasMaxLength(150);
        builder.Property(a => a.RegistrationNumber).HasMaxLength(50);

        // ── Volunteer-specific ──────────────────────────────────
        builder.Property(a => a.VehicleDetails).HasMaxLength(100);
        builder.Property(a => a.VehicleType).HasMaxLength(50);

        builder.Property(a => a.IsAvailable)
            .HasDefaultValue(false)
            .IsRequired();

        // ═════════════════════════════════════════════════════════
        //  Backing fields — collection navigations
        // ═════════════════════════════════════════════════════════
        void UseFieldAccess(string navName)
        {
            var nav = builder.Metadata.FindNavigation(navName);
            nav?.SetPropertyAccessMode(PropertyAccessMode.Field);
        }

        UseFieldAccess(nameof(Account.DonationsMade));
        UseFieldAccess(nameof(Account.DonationsReceived));
        UseFieldAccess(nameof(Account.DonationsHandled));
        UseFieldAccess(nameof(Account.DeliveryMissions));
        UseFieldAccess(nameof(Account.RefreshTokens));
        UseFieldAccess(nameof(Account.PointTransactions));
        UseFieldAccess(nameof(Account.ReviewsGiven));
        UseFieldAccess(nameof(Account.ReviewsReceived));
        UseFieldAccess(nameof(Account.Notifications));
        UseFieldAccess(nameof(Account.PenaltyLogs));

        // ═════════════════════════════════════════════════════════
        //  Relationships
        // ═════════════════════════════════════════════════════════
        builder.HasOne(a => a.City)
            .WithMany()
            .HasForeignKey(a => a.CityId)
            .OnDelete(DeleteBehavior.Restrict);

        // ═════════════════════════════════════════════════════════
        //  Indexes
        // ═════════════════════════════════════════════════════════

        // Phone: non-unique
        builder.HasIndex(a => a.PhoneNumber)
            .HasDatabaseName("IX_Accounts_PhoneNumber");

        // ✅ Email unique فقط للحسابات غير المحذوفة
        builder.HasIndex(a => new { a.Email, a.IsDeleted })
            .IsUnique()
            .HasDatabaseName("IX_Accounts_Email_IsDeleted");

        // ✅ UserName unique فقط للحسابات غير المحذوفة
        builder.HasIndex(a => new { a.UserName, a.IsDeleted })
            .IsUnique()
            .HasDatabaseName("IX_Accounts_UserName_IsDeleted");

        // ✅ NormalizedEmail / NormalizedUserName كمان (Identity بيستخدمهم في الـ Login)
        builder.HasIndex(a => new { a.NormalizedEmail, a.IsDeleted })
            .IsUnique()
            .HasDatabaseName("IX_Accounts_NormalizedEmail_IsDeleted");

        builder.HasIndex(a => new { a.NormalizedUserName, a.IsDeleted })
            .IsUnique()
            .HasDatabaseName("IX_Accounts_NormalizedUserName_IsDeleted");

        builder.HasIndex(a => a.Role)
            .HasDatabaseName("IX_Accounts_Role");

        builder.HasIndex(a => a.CityId)
            .HasDatabaseName("IX_Accounts_CityId");

        builder.HasIndex(a => new { a.IsActive, a.IsDeleted })
            .HasDatabaseName("IX_Accounts_IsActive_IsDeleted");

        builder.HasIndex(a => new { a.Role, a.IsAvailable, a.CityId })
            .HasDatabaseName("IX_Accounts_Role_Available_City");

        // ✅ Soft delete global filter
        builder.HasQueryFilter(a => !a.IsDeleted);
    }
}