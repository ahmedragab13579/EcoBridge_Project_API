using EcoBridge.Domain.Constants;
using EcoBridge.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcoBridge.Infrastructure.Data.Configurations;

public class PenaltyLogConfiguration : IEntityTypeConfiguration<PenaltyLog>
{
    public void Configure(EntityTypeBuilder<PenaltyLog> builder)
    {
        // ═════════════════════════════════════════════════════════
        //  Table
        // ═════════════════════════════════════════════════════════
        builder.ToTable("PenaltyLogs", t =>
        {
            t.HasCheckConstraint(
                "CK_PenaltyLogs_Amount_Positive",
                "[Amount] > 0");

            t.HasCheckConstraint(
                "CK_PenaltyLogs_ReasonCode_NotEmpty",
                "LEN(LTRIM(RTRIM([ReasonCode]))) > 0");
        });

        // ═════════════════════════════════════════════════════════
        //  Key
        // ═════════════════════════════════════════════════════════
        builder.HasKey(p => p.Id);

        // ═════════════════════════════════════════════════════════
        //  Scalar properties
        // ═════════════════════════════════════════════════════════
        builder.Property(p => p.AccountId)
            .IsRequired();

        builder.Property(p => p.Amount)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(p => p.ReasonCode)
            .HasMaxLength(DomainConstants.Name.MaxCodeLength)
            .IsRequired();

        builder.Property(p => p.MissionId)
            .IsRequired(false);

        builder.Property(p => p.CreatedAtUtc)
            .HasColumnType("datetime2")
            .IsRequired();

        // ═════════════════════════════════════════════════════════
        //  Relationships
        // ═════════════════════════════════════════════════════════

        // ── PenaltyLog → Account (NoAction) ──────────────────────
        builder.HasOne(p => p.Account)
            .WithMany(a => a.PenaltyLogs)
            .HasForeignKey(p => p.AccountId)
            .OnDelete(DeleteBehavior.NoAction);

        // ── PenaltyLog → DeliveryMission (NoAction) ─────────────
        builder.HasOne(p => p.Mission)
            .WithMany()
            .HasForeignKey(p => p.MissionId)
            .OnDelete(DeleteBehavior.NoAction);

        // ═════════════════════════════════════════════════════════
        //  Indexes
        // ═════════════════════════════════════════════════════════
        builder.HasIndex(p => p.AccountId)
            .HasDatabaseName("IX_PenaltyLogs_AccountId");

        builder.HasIndex(p => p.MissionId)
            .HasDatabaseName("IX_PenaltyLogs_MissionId");

        builder.HasIndex(p => p.CreatedAtUtc)
            .HasDatabaseName("IX_PenaltyLogs_CreatedAtUtc");

        builder.HasIndex(p => p.ReasonCode)
            .HasDatabaseName("IX_PenaltyLogs_ReasonCode");

        builder.HasIndex(p => new { p.AccountId, p.CreatedAtUtc })
            .HasDatabaseName("IX_PenaltyLogs_AccountId_CreatedAtUtc");
    }
}