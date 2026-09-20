using EcoBridge.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcoBridge.Infrastructure.Data.Configurations;

public class PointTransactionConfiguration : IEntityTypeConfiguration<PointTransaction>
{
    public void Configure(EntityTypeBuilder<PointTransaction> builder)
    {
        // ═════════════════════════════════════════════════════════
        //  Table
        // ═════════════════════════════════════════════════════════
        builder.ToTable("PointTransactions", t =>
        {
            t.HasCheckConstraint(
                "CK_PointTransactions_Amount_NonZero",
                "[Amount] <> 0");

            t.HasCheckConstraint(
                "CK_PointTransactions_Amount_Absolute_Limit",
                "ABS([Amount]) <= 1000000");

            // Sign enforced per TransactionType:
            //   1 = MissionCompleted → Amount > 0
            //   2 = Penalty          → Amount < 0
            //   3 = PointsRedeemed   → Amount < 0
            t.HasCheckConstraint(
                "CK_PointTransactions_Sign_Per_Type",
                "([TransactionType] = 1 AND [Amount] > 0) " +
                "OR ([TransactionType] = 2 AND [Amount] < 0) " +
                "OR ([TransactionType] = 3 AND [Amount] < 0)");
        });

        // ═════════════════════════════════════════════════════════
        //  Key
        // ═════════════════════════════════════════════════════════
        builder.HasKey(pt => pt.Id);

        // ═════════════════════════════════════════════════════════
        //  Scalar properties
        // ═════════════════════════════════════════════════════════
        builder.Property(pt => pt.UserId)
            .IsRequired();

        builder.Property(pt => pt.Amount)
            .IsRequired();

        builder.Property(pt => pt.TransactionType)
            .HasConversion<byte>()
            .HasColumnType("tinyint")
            .IsRequired();

        builder.Property(pt => pt.ReferenceId)
            .IsRequired(false);

        builder.Property(pt => pt.CreatedAt)
            .HasColumnType("datetime2")
            .IsRequired();

        // ═════════════════════════════════════════════════════════
        //  Relationships
        // ═════════════════════════════════════════════════════════
        builder.HasOne(pt => pt.User)
            .WithMany(a => a.PointTransactions)
            .HasForeignKey(pt => pt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // ═════════════════════════════════════════════════════════
        //  Indexes
        // ═════════════════════════════════════════════════════════
        builder.HasIndex(pt => pt.UserId)
            .HasDatabaseName("IX_PointTransactions_UserId");

        builder.HasIndex(pt => pt.CreatedAt)
            .HasDatabaseName("IX_PointTransactions_CreatedAt");

        builder.HasIndex(pt => pt.TransactionType)
            .HasDatabaseName("IX_PointTransactions_TransactionType");

        builder.HasIndex(pt => pt.ReferenceId)
            .HasDatabaseName("IX_PointTransactions_ReferenceId");

        builder.HasIndex(pt => new { pt.UserId, pt.CreatedAt })
            .HasDatabaseName("IX_PointTransactions_UserId_CreatedAt");
    }
}