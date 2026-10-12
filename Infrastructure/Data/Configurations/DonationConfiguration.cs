using EcoBridge.Domain.Constants;
using EcoBridge.Domain.Models.Donation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcoBridge.Infrastructure.Data.Configurations;

public class DonationConfiguration : IEntityTypeConfiguration<Donation>
{
    public void Configure(EntityTypeBuilder<Donation> builder)
    {
        // ═════════════════════════════════════════════════════════
        //  Table + Check Constraints
        // ═════════════════════════════════════════════════════════
        builder.ToTable("Donations", t =>
        {
            t.HasCheckConstraint(
                "CK_Donations_FoodType_NotEmpty",
                "LEN(LTRIM(RTRIM([FoodType]))) > 0");

            t.HasCheckConstraint(
                "CK_Donations_Quantity_NotEmpty",
                "LEN(LTRIM(RTRIM([QuantityDescription]))) > 0");

            t.HasCheckConstraint(
                "CK_Donations_PickupLocation_NotEmpty",
                "LEN(LTRIM(RTRIM([PickupLocation]))) > 0");

            t.HasCheckConstraint(
                "CK_Donations_Weight_Positive",
                "[WeightKg] IS NULL OR ([WeightKg] > 0 AND [WeightKg] <= 100000)");

            t.HasCheckConstraint(
                "CK_Donations_ExpiryDate_Future",
                "[ExpiryDate] >= [CreatedAt]");
        });

        builder.HasKey(d => d.Id);

        // ═════════════════════════════════════════════════════════
        //  Scalar properties
        // ═════════════════════════════════════════════════════════
        builder.Property(d => d.DonorId).IsRequired();
        builder.Property(d => d.CharityId).IsRequired(false);
        builder.Property(d => d.VolunteerId).IsRequired(false);
        builder.Property(d => d.CategoryId).IsRequired();
        builder.Property(d => d.CityId).IsRequired();

        builder.Property(d => d.Title).HasMaxLength(DomainConstants.Name.MaxNameLength);
        builder.Property(d => d.Description).HasMaxLength(DomainConstants.Body.MaxBodyLength);

        builder.Property(d => d.FoodType)
            .HasMaxLength(DomainConstants.Body.MaxBodyLength)
            .IsRequired();

        builder.Property(d => d.QuantityDescription)
            .HasMaxLength(DomainConstants.Body.MaxBodyLength)
            .IsRequired();

        builder.Property(d => d.WeightKg)
            .HasColumnType("decimal(10,2)");

        builder.Property(d => d.ExpiryDate)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(d => d.PickupLocation)
            .HasMaxLength(DomainConstants.Body.MaxLocationLength)
            .IsRequired();

        builder.Property(d => d.ImageUrl).HasMaxLength(500);

        // ✅ string enum
        builder.Property(d => d.Status)
            .HasConversion<string>()
            .HasMaxLength(DomainConstants.Name.MaxCodeLength)
            .IsRequired();

        builder.Property(d => d.CreatedAt)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(d => d.CancelledAtUtc).HasColumnType("datetime2");
        builder.Property(d => d.ExpiredAtUtc).HasColumnType("datetime2");

        builder.Property(d => d.CancellationReason).HasMaxLength(500);

        builder.Property(d => d.IsDeleted)
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(d => d.DeletedAt).HasColumnType("datetime2");

        // ✅ IsRowVersion() كفاية
        builder.Property(d => d.RowVersion)
            .IsRowVersion();

        // ═════════════════════════════════════════════════════════
        //  Relationships
        // ═════════════════════════════════════════════════════════

        builder.HasOne(d => d.Donor)
            .WithMany(a => a.DonationsMade)
            .HasForeignKey(d => d.DonorId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(d => d.Charity)
            .WithMany(a => a.DonationsReceived)
            .HasForeignKey(d => d.CharityId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(d => d.Volunteer)
            .WithMany(a => a.DonationsHandled)
            .HasForeignKey(d => d.VolunteerId)
            .OnDelete(DeleteBehavior.NoAction);

        // ✅ Category: one-way relationship
        builder.HasOne(d => d.Category)
            .WithMany()
            .HasForeignKey(d => d.CategoryId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(d => d.City)
            .WithMany()
            .HasForeignKey(d => d.CityId)
            .OnDelete(DeleteBehavior.NoAction);

        // ═════════════════════════════════════════════════════════
        //  Indexes
        // ═════════════════════════════════════════════════════════
        builder.HasIndex(d => d.CreatedAt)
            .HasDatabaseName("IX_Donations_CreatedAt");

        builder.HasIndex(d => d.ExpiryDate)
            .HasDatabaseName("IX_Donations_ExpiryDate");

        builder.HasIndex(d => d.CategoryId)
            .HasDatabaseName("IX_Donations_CategoryId");

        builder.HasIndex(d => d.CityId)
            .HasDatabaseName("IX_Donations_CityId");

        builder.HasIndex(d => d.DonorId)
            .HasDatabaseName("IX_Donations_DonorId");

        builder.HasIndex(d => new { d.Status, d.ExpiryDate })
            .HasDatabaseName("IX_Donations_Status_ExpiryDate");

        builder.HasIndex(d => new { d.Status, d.CityId })
            .HasDatabaseName("IX_Donations_Status_CityId");

        builder.HasIndex(d => new { d.CharityId, d.Status })
            .HasDatabaseName("IX_Donations_CharityId_Status");

        builder.HasIndex(d => new { d.VolunteerId, d.Status })
            .HasDatabaseName("IX_Donations_VolunteerId_Status");

    }
}