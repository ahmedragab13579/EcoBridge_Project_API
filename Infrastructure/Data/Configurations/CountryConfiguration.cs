using EcoBridge.Domain.Constants;
using EcoBridge.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcoBridge.Infrastructure.Data.Configurations;

public class CountryConfiguration : IEntityTypeConfiguration<Country>
{
    public void Configure(EntityTypeBuilder<Country> builder)
    {
        // ═════════════════════════════════════════════════════════
        //  Table
        // ═════════════════════════════════════════════════════════
        builder.ToTable("Countries", t =>
        {
            t.HasCheckConstraint(
                "CK_Countries_Code_NotEmpty",
                "LEN(LTRIM(RTRIM([Code]))) > 0");

            t.HasCheckConstraint(
                "CK_Countries_NameAr_NotEmpty",
                "LEN(LTRIM(RTRIM([NameAr]))) > 0");

            t.HasCheckConstraint(
                "CK_Countries_NameEn_NotEmpty",
                "LEN(LTRIM(RTRIM([NameEn]))) > 0");
        });

        // ═════════════════════════════════════════════════════════
        //  Key
        // ═════════════════════════════════════════════════════════
        builder.HasKey(c => c.Id);

        // ═════════════════════════════════════════════════════════
        //  Scalar properties
        // ═════════════════════════════════════════════════════════
        builder.Property(c => c.Code)
            .HasMaxLength(DomainConstants.Name.MaxCodeLength)
            .IsRequired();

        builder.Property(c => c.NameAr)
            .HasMaxLength(DomainConstants.Name.MaxNameLength)
            .IsRequired();

        builder.Property(c => c.NameEn)
            .HasMaxLength(DomainConstants.Name.MaxNameLength)
            .IsRequired();

        // ═════════════════════════════════════════════════════════
        //  Backing field
        // ═════════════════════════════════════════════════════════
        builder.Navigation(c => c.Governorates)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // ═════════════════════════════════════════════════════════
        //  Relationships
        // ═════════════════════════════════════════════════════════
        // Country → Governorates configured from the Governorate side.

        // ═════════════════════════════════════════════════════════
        //  Indexes
        // ═════════════════════════════════════════════════════════
        builder.HasIndex(c => c.Code)
            .IsUnique()
            .HasDatabaseName("UX_Countries_Code");

        builder.HasIndex(c => c.NameAr)
            .IsUnique()
            .HasDatabaseName("UX_Countries_NameAr");

        builder.HasIndex(c => c.NameEn)
            .IsUnique()
            .HasDatabaseName("UX_Countries_NameEn");
    }
}