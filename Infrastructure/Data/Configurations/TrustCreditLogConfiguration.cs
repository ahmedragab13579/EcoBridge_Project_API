using EcoBridge.Domain.Constants;
using EcoBridge.Domain.Models;
using EcoBridge.Domain.Models.Mission;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcoBridge.Infrastructure.Data.Configurations;

public class TrustCreditLogConfiguration : IEntityTypeConfiguration<TrustCreditLog>
{
    public void Configure(EntityTypeBuilder<TrustCreditLog> builder)
    {
        // ═════════════════════════════════════════════════════════
        //  Table + Check Constraints
        // ═════════════════════════════════════════════════════════
        builder.ToTable("TrustCreditLogs", t =>
        {
            // ✅ الـ Model بيمنع amount <= 0، فالـ DB لازم تعمل نفس الحاجة
            t.HasCheckConstraint(
                "CK_TrustCreditLogs_Amount_Positive",
                "[Amount] > 0");

            t.HasCheckConstraint(
                "CK_TrustCreditLogs_Amount_MaxLimit",
                "[Amount] <= 1000000");

            t.HasCheckConstraint(
                "CK_TrustCreditLogs_ReasonCode_NotEmpty",
                "LEN(LTRIM(RTRIM([ReasonCode]))) > 0");
        });

        // ═════════════════════════════════════════════════════════
        //  Key
        // ═════════════════════════════════════════════════════════
        builder.HasKey(tc => tc.Id);

        // ═════════════════════════════════════════════════════════
        //  Scalar properties
        // ═════════════════════════════════════════════════════════
        builder.Property(tc => tc.Id)
            .ValueGeneratedOnAdd();

        builder.Property(tc => tc.AccountId)
            .IsRequired();

        builder.Property(tc => tc.Amount)
            .HasColumnType("decimal")
            .IsRequired();

        builder.Property(tc => tc.ReasonCode)
            .HasMaxLength(DomainConstants.Name.MaxCodeLength)
            .IsRequired();

        builder.Property(tc => tc.MissionId)
            .IsRequired(false);

        builder.Property(tc => tc.CreatedAtUtc)
            .HasColumnType("datetime2")
            .IsRequired();

        // ═════════════════════════════════════════════════════════
        //  Relationships
        // ═════════════════════════════════════════════════════════

        // ── TrustCreditLog → Account (Restrict) ─────────────────
        // ✅ Restrict مش Cascade (نفس منطق PenaltyLog + PointTransaction)
        builder.HasOne(tc => tc.Account)
            .WithMany()   // ⚠️ شوف تحت: لو Account عنده Navigation، حددها
            .HasForeignKey(tc => tc.AccountId)
            .OnDelete(DeleteBehavior.Restrict);

        // ── TrustCreditLog → DeliveryMission (Restrict) ─────────
        builder.HasOne<DeliveryMission>()
            .WithMany()
            .HasForeignKey(tc => tc.MissionId)
            .OnDelete(DeleteBehavior.Restrict);

        // ═════════════════════════════════════════════════════════
        //  Indexes
        // ═════════════════════════════════════════════════════════
        builder.HasIndex(tc => tc.AccountId)
            .HasDatabaseName("IX_TrustCreditLogs_AccountId");

        builder.HasIndex(tc => tc.MissionId)
            .HasDatabaseName("IX_TrustCreditLogs_MissionId");

        builder.HasIndex(tc => tc.CreatedAtUtc)
            .HasDatabaseName("IX_TrustCreditLogs_CreatedAtUtc");

        builder.HasIndex(tc => tc.ReasonCode)
            .HasDatabaseName("IX_TrustCreditLogs_ReasonCode");

        // Composite index: "كل نقاط الثقة لحساب معين مرتبة بالتاريخ"
        builder.HasIndex(tc => new { tc.AccountId, tc.CreatedAtUtc })
            .HasDatabaseName("IX_TrustCreditLogs_AccountId_CreatedAtUtc");
    }
}