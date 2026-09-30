using EcoBridge.Domain.Constants;
using EcoBridge.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcoBridge.Infrastructure.Data.Configurations;

public class ReviewReasonConfiguration : IEntityTypeConfiguration<ReviewReason>
{
    public void Configure(EntityTypeBuilder<ReviewReason> builder)
    {
        // ═════════════════════════════════════════════════════════
        //  Table + Check Constraints
        // ═════════════════════════════════════════════════════════
        builder.ToTable("ReviewReasons", t =>
        {
            t.HasCheckConstraint(
                "CK_ReviewReasons_Code_NotEmpty",
                "LEN(LTRIM(RTRIM([Code]))) > 0");


            t.HasCheckConstraint(
                "CK_ReviewReasons_FromRole_Not_ToRole",
                "[FromRole] <> [ToRole]");
        });

        builder.HasKey(rr => rr.Id);

        // ═════════════════════════════════════════════════════════
        //  Scalar properties
        // ═════════════════════════════════════════════════════════
        builder.Property(rr => rr.Code)
            .HasMaxLength(DomainConstants.Name.MaxCodeLength)
            .IsRequired();

        builder.Property(rr => rr.FromRole)
            .HasConversion<string>()
            .HasMaxLength(DomainConstants.Name.MaxCodeLength)   
            .IsRequired();

        builder.Property(rr => rr.ToRole)
            .HasConversion<string>()
            .HasMaxLength(DomainConstants.Name.MaxCodeLength)
            .IsRequired();

        builder.Property(rr => rr.IsPositive)
            .IsRequired();

        // ═════════════════════════════════════════════════════════
        //  Backing field
        // ═════════════════════════════════════════════════════════
        builder.Navigation(rr => rr.Reviews)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // ═════════════════════════════════════════════════════════
        //  Relationships
        // ═════════════════════════════════════════════════════════
        // ReviewReason → Reviews configured from the Review side.

        // ═════════════════════════════════════════════════════════
        //  Indexes
        // ═════════════════════════════════════════════════════════
        builder.HasIndex(rr => rr.Code)
            .IsUnique()
            .HasDatabaseName("UX_ReviewReasons_Code");

        builder.HasIndex(rr => new { rr.FromRole, rr.ToRole, rr.IsPositive })
            .HasDatabaseName("IX_ReviewReasons_FromRole_ToRole_IsPositive");
    }
}