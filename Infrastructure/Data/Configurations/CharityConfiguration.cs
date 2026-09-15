using EcoBridge.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcoBridge.Infrastructure.Data.Configurations;

public class CharityConfiguration : IEntityTypeConfiguration<Charity>
{
    public void Configure(EntityTypeBuilder<Charity> builder)
    {
        builder.HasKey(x => x.AccountId);

        builder.Property(x => x.OrganizationName).HasColumnType("nvarchar(150)").IsRequired();
        builder.Property(x => x.RegistrationNumber).HasColumnType("nvarchar(50)").IsRequired();
        builder.Property(x => x.Address).HasColumnType("nvarchar(255)");
        builder.Property(x => x.IsDeleted).HasDefaultValue(false);
        builder.Property(x => x.DeletedAt).HasColumnType("datetime2");

        // Global Query Filter for Soft Delete
        builder.HasQueryFilter(x => !x.IsDeleted);

        builder.HasIndex(x => x.RegistrationNumber);
        builder.HasIndex(x => x.IsDeleted);
    }
}
