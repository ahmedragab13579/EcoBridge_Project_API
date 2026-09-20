using EcoBridge.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcoBridge.Infrastructure.Data.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        // ═════════════════════════════════════════════════════════
        //  Table
        // ═════════════════════════════════════════════════════════
        builder.ToTable("Categories", t =>
        {
            t.HasCheckConstraint(
                "CK_Categories_NameAr_NotEmpty",
                "LEN(LTRIM(RTRIM([NameAr]))) > 0");

            t.HasCheckConstraint(
                "CK_Categories_NameEn_NotEmpty",
                "LEN(LTRIM(RTRIM([NameEn]))) > 0");
        });

        // ═════════════════════════════════════════════════════════
        //  Key
        // ═════════════════════════════════════════════════════════
        builder.HasKey(c => c.Id);

        // ═════════════════════════════════════════════════════════
        //  Scalar properties
        // ═════════════════════════════════════════════════════════
        builder.Property(c => c.NameAr)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(c => c.NameEn)
            .HasMaxLength(100)
            .IsRequired();

        // ═════════════════════════════════════════════════════════
        //  Backing field
        // ═════════════════════════════════════════════════════════
        builder.Navigation(c => c.Donations)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // ═════════════════════════════════════════════════════════
        //  Relationships
        // ═════════════════════════════════════════════════════════
        // ⚠️ Category → Donations is configured from the Donation side
        //    (Donation is the FK holder). See DonationConfiguration.

        // ═════════════════════════════════════════════════════════
        //  Indexes
        // ═════════════════════════════════════════════════════════
        builder.HasIndex(c => c.NameEn)
            .IsUnique()
            .HasDatabaseName("UX_Categories_NameEn");

        builder.HasIndex(c => c.NameAr)
            .IsUnique()
            .HasDatabaseName("UX_Categories_NameAr");
    }
}