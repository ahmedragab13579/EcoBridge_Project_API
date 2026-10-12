using EcoBridge.Domain.Constants;
using EcoBridge.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcoBridge.Infrastructure.Data.Configurations;

public class PointTransactionConfiguration : IEntityTypeConfiguration<PointTransaction>
{
    public void Configure(EntityTypeBuilder<PointTransaction> builder)
    {
        // ═════════════════════════════════════════════════════════
        //  Table + Check Constraints
        // ═════════════════════════════════════════════════════════
        builder.ToTable("PointTransactions", t =>
        {
            t.HasCheckConstraint(
                "CK_PointTransactions_Amount_NonZero",
                "[Amount] <> 0");

            t.HasCheckConstraint(
                "CK_PointTransactions_Amount_Absolute_Limit",
                "ABS([Amount]) <= 1000000");

            t.HasCheckConstraint(
        "CK_PointTransactions_Sign_Per_Type",
        "([TransactionType] = 'MissionCompleted' AND [Amount] > 0) " +
        "OR ([TransactionType] = 'PointsRedeemed' AND [Amount] < 0) " +
        "OR ([TransactionType] = 'Penalty' AND [Amount] < 0) " +
        "OR ([TransactionType] = 'UnassignCompensation' AND [Amount] > 0)");
        });

        builder.HasKey(pt => pt.Id);

        // ═════════════════════════════════════════════════════════
        //  Scalar properties
        // ═════════════════════════════════════════════════════════
        builder.Property(pt => pt.UserId)
            .IsRequired();

        // ✅ حدد نوع الـ Amount صريح
        builder.Property(pt => pt.Amount)
    .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(pt => pt.TransactionType)
            .HasConversion<string>()
            .HasMaxLength(DomainConstants.Name.MaxCodeLength)
            .IsRequired();

        // ✅ حدد نوع ReferenceId حسب النوع الفعلي
        builder.Property(pt => pt.ReferenceId)
            .HasMaxLength(DomainConstants.Name.MaxNameLength)  // لو string، ولو int/Guid شيل السطر ده
            .IsRequired(false);

        builder.Property(pt => pt.CreatedAt)
            .HasColumnType("datetime2")
            .IsRequired();

        // ═════════════════════════════════════════════════════════
        //  Relationships
        // ═════════════════════════════════════════════════════════
        // ✅ NoAction بدل NoAction (مهم للـ Audit Trail + Soft Delete)
        builder.HasOne(pt => pt.User)
            .WithMany(a => a.PointTransactions)
            .HasForeignKey(pt => pt.UserId)
            .OnDelete(DeleteBehavior.NoAction);

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