using EcoBridge.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcoBridge.Infrastructure.Data.Configurations;

public class DonorConfiguration : IEntityTypeConfiguration<Donor>
{
    public void Configure(EntityTypeBuilder<Donor> builder)
    {
        builder.HasKey(x => x.AccountId);

        builder.Property(x => x.UserType).HasColumnType("tinyint").IsRequired();
        builder.Property(x => x.OrganizationName).HasColumnType("nvarchar(150)");
        builder.Property(x => x.Address).HasColumnType("nvarchar(255)");
    }
}
