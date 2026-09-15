using EcoBridge.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcoBridge.Infrastructure.Data.Configurations;

public class CityConfiguration : IEntityTypeConfiguration<City>
{
    public void Configure(EntityTypeBuilder<City> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code).HasColumnType("nvarchar(10)").IsRequired();
        builder.Property(x => x.NameAr).HasColumnType("nvarchar(100)").IsRequired();
        builder.Property(x => x.NameEn).HasColumnType("nvarchar(100)").IsRequired();

        builder.HasOne(x => x.Governorate)
            .WithMany(x => x.Cities)
            .HasForeignKey(x => x.GovernorateId)
            .OnDelete(DeleteBehavior.Restrict);

        // Composite index per PRD §6.C
        builder.HasIndex(x => new { x.GovernorateId, x.NameAr });
        builder.HasIndex(x => x.GovernorateId);
        builder.HasIndex(x => x.Code);
    }
}
