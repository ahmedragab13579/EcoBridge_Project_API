using EcoBridge.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcoBridge.Infrastructure.Data.Configurations;

public class DonationConfiguration : IEntityTypeConfiguration<Donation>
{
    public void Configure(EntityTypeBuilder<Donation> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title).HasColumnType("nvarchar(200)");
        builder.Property(x => x.Description).HasColumnType("nvarchar(1000)");
        builder.Property(x => x.FoodType).HasColumnType("nvarchar(100)").IsRequired();
        builder.Property(x => x.Quantity).HasColumnType("nvarchar(50)").IsRequired();
        builder.Property(x => x.Weight).HasColumnType("decimal(10,2)");
        builder.Property(x => x.ExpiryDate).HasColumnType("datetime2").IsRequired();
        builder.Property(x => x.PickupLocation).HasColumnType("nvarchar(255)").IsRequired();
        builder.Property(x => x.ImageUrl).HasColumnType("nvarchar(max)");
        builder.Property(x => x.Status).HasColumnType("tinyint").IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnType("datetime2").IsRequired();
        builder.Property(x => x.CancellationReason).HasColumnType("nvarchar(500)");
        builder.Property(x => x.IsDeleted).HasDefaultValue(false);
        builder.Property(x => x.DeletedAt).HasColumnType("datetime2");

        builder.HasOne(x => x.Donor)
            .WithMany(x => x.Donations)
            .HasForeignKey(x => x.DonorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Category)
            .WithMany(x => x.Donations)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.Charity)
            .WithMany(x => x.Donations)
            .HasForeignKey(x => x.CharityId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.Volunteer)
            .WithMany(x => x.Donations)
            .HasForeignKey(x => x.VolunteerId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.City)
            .WithMany()
            .HasForeignKey(x => x.CityId)
            .OnDelete(DeleteBehavior.SetNull);

        // Global Soft Delete Filter
        builder.HasQueryFilter(x => !x.IsDeleted);

        // Indexes
        builder.HasIndex(x => x.CreatedAt);
        builder.HasIndex(x => x.ExpiryDate);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.IsDeleted);
        builder.HasIndex(x => x.CategoryId);
        builder.HasIndex(x => x.CityId);
        builder.HasIndex(x => x.DonorId);
        builder.HasIndex(x => x.CharityId);
        builder.HasIndex(x => x.VolunteerId);
    }
}
