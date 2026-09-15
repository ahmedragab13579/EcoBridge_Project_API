using EcoBridge.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcoBridge.Infrastructure.Data.Configurations;

public class GovernorateConfiguration : IEntityTypeConfiguration<Governorate>
{
    public void Configure(EntityTypeBuilder<Governorate> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code).HasColumnType("nvarchar(10)").IsRequired();
        builder.Property(x => x.NameAr).HasColumnType("nvarchar(100)").IsRequired();
        builder.Property(x => x.NameEn).HasColumnType("nvarchar(100)").IsRequired();

        builder.HasOne(x => x.Country)
            .WithMany(x => x.Governorates)
            .HasForeignKey(x => x.CountryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.CountryId);
        builder.HasIndex(x => x.Code);
    }
}
