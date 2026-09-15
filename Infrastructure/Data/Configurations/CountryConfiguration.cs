using EcoBridge.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcoBridge.Infrastructure.Data.Configurations;

public class CountryConfiguration : IEntityTypeConfiguration<Country>
{
    public void Configure(EntityTypeBuilder<Country> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code).HasColumnType("nvarchar(10)").IsRequired();
        builder.Property(x => x.NameAr).HasColumnType("nvarchar(100)").IsRequired();
        builder.Property(x => x.NameEn).HasColumnType("nvarchar(100)").IsRequired();

        builder.HasIndex(x => x.Code).IsUnique();
    }
}
