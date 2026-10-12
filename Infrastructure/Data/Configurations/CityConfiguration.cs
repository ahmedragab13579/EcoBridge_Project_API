using EcoBridge.Domain.Constants;
using EcoBridge.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcoBridge.Infrastructure.Data.Configurations;

public class CityConfiguration : IEntityTypeConfiguration<City>
{
    //Microsoft.Data.SqlClient.SqlException:
    //'The INSERT statement conflicted with the FOREIGN KEY constraint "FK_Donations_Cities_CityId".
    //The conflict occurred in database "master", table "dbo.Cities", column 'Id'.'
    public void Configure(EntityTypeBuilder<City> builder)
    {
        // ═════════════════════════════════════════════════════════
        //  Table
        // ═════════════════════════════════════════════════════════
        builder.ToTable("Cities", t =>
        {
            t.HasCheckConstraint(
                "CK_Cities_Code_NotEmpty",
                "LEN(LTRIM(RTRIM([Code]))) > 0");

            t.HasCheckConstraint(
                "CK_Cities_NameAr_NotEmpty",
                "LEN(LTRIM(RTRIM([NameAr]))) > 0");

            t.HasCheckConstraint(
                "CK_Cities_NameEn_NotEmpty",
                "LEN(LTRIM(RTRIM([NameEn]))) > 0");
        });

        // ═════════════════════════════════════════════════════════
        //  Key
        // ═════════════════════════════════════════════════════════
        builder.HasKey(c => c.Id);

        // ═════════════════════════════════════════════════════════
        //  Scalar properties
        // ═════════════════════════════════════════════════════════
        builder.Property(c => c.GovernorateId)
            .IsRequired();

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
        //  Relationships
        // ═════════════════════════════════════════════════════════
        builder.HasOne(c => c.Governorate)
            .WithMany(g => g.Cities)
            .HasForeignKey(c => c.GovernorateId)
            .OnDelete(DeleteBehavior.NoAction);

        // ═════════════════════════════════════════════════════════
        //  Indexes
        // ═════════════════════════════════════════════════════════
        builder.HasIndex(c => c.GovernorateId)
            .HasDatabaseName("IX_Cities_GovernorateId");

        builder.HasIndex(c => c.Code)
            .IsUnique()
            .HasDatabaseName("UX_Cities_Code");

        builder.HasIndex(c => new { c.GovernorateId, c.NameAr })
            .IsUnique()
            .HasDatabaseName("UX_Cities_GovernorateId_NameAr");

        builder.HasIndex(c => new { c.GovernorateId, c.NameEn })
            .IsUnique()
            .HasDatabaseName("UX_Cities_GovernorateId_NameEn");
    }
}