using EcoBridge.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcoBridge.Infrastructure.Data.Configurations;

public class VolunteerConfiguration : IEntityTypeConfiguration<Volunteer>
{
    public void Configure(EntityTypeBuilder<Volunteer> builder)
    {
        builder.HasKey(x => x.AccountId);

        builder.Property(x => x.VehicleDetails).HasColumnType("nvarchar(100)");
        builder.Property(x => x.VehicleType).HasColumnType("nvarchar(50)");
        builder.Property(x => x.IsAvailable).HasDefaultValue(true);
        builder.Property(x => x.IsDeleted).HasDefaultValue(false);
        builder.Property(x => x.DeletedAt).HasColumnType("datetime2");

        // Global Query Filter for Soft Delete
        builder.HasQueryFilter(x => !x.IsDeleted);

        builder.HasIndex(x => x.IsAvailable);
        builder.HasIndex(x => x.IsDeleted);
    }
}
