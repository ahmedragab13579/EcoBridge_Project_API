using EcoBridge.Domain.Constants;
using EcoBridge.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcoBridge.Infrastructure.Data.Configurations;

public class GovernorateConfiguration : IEntityTypeConfiguration<Governorate>
{
    public void Configure(EntityTypeBuilder<Governorate> builder)
    {
        // ═════════════════════════════════════════════════════════
        //  Table
        // ═════════════════════════════════════════════════════════
        builder.ToTable("Governorates", t =>
        {
            t.HasCheckConstraint(
                "CK_Governorates_Code_NotEmpty",
                "LEN(LTRIM(RTRIM([Code]))) > 0");

            t.HasCheckConstraint(
                "CK_Governorates_NameAr_NotEmpty",
                "LEN(LTRIM(RTRIM([NameAr]))) > 0");

            t.HasCheckConstraint(
                "CK_Governorates_NameEn_NotEmpty",
                "LEN(LTRIM(RTRIM([NameEn]))) > 0");
        });

        // ═════════════════════════════════════════════════════════
        //  Key
        // ═════════════════════════════════════════════════════════
        builder.HasKey(g => g.Id);

        // ═════════════════════════════════════════════════════════
        //  Scalar properties
        // ═════════════════════════════════════════════════════════
        builder.Property(g => g.CountryId)
            .IsRequired();

        builder.Property(g => g.Code)
            .HasMaxLength(DomainConstants.Name.MaxCodeLength)
            .IsRequired();

        builder.Property(g => g.NameAr)
            .HasMaxLength(DomainConstants.Name.MaxNameLength)
            .IsRequired();

        builder.Property(g => g.NameEn)
            .HasMaxLength(DomainConstants.Name.MaxNameLength)
            .IsRequired();

        // ═════════════════════════════════════════════════════════
        //  Backing field
        // ═════════════════════════════════════════════════════════
        builder.Navigation(g => g.Cities)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // ═════════════════════════════════════════════════════════
        //  Relationships
        // ═════════════════════════════════════════════════════════
        builder.HasOne(g => g.Country)
            .WithMany(c => c.Governorates)
            .HasForeignKey(g => g.CountryId)
            .OnDelete(DeleteBehavior.Restrict);

        // ═════════════════════════════════════════════════════════
        //  Indexes
        // ═════════════════════════════════════════════════════════
        builder.HasIndex(g => g.CountryId)
            .HasDatabaseName("IX_Governorates_CountryId");

        builder.HasIndex(g => g.Code)
            .IsUnique()
            .HasDatabaseName("UX_Governorates_Code");

        builder.HasIndex(g => new { g.CountryId, g.NameAr })
            .IsUnique()
            .HasDatabaseName("UX_Governorates_CountryId_NameAr");

        builder.HasIndex(g => new { g.CountryId, g.NameEn })
            .IsUnique()
            .HasDatabaseName("UX_Governorates_CountryId_NameEn");
    }
}