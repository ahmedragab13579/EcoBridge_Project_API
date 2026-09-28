using EcoBridge.Domain.Models.Donation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcoBridge.Infrastructure.Data.Configurations;

public class DonationConfiguration : IEntityTypeConfiguration<Donation>
{
    public void Configure(EntityTypeBuilder<Donation> builder)
    {
        // ═════════════════════════════════════════════════════════
        //  Table
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
                "[ExpiryDate] > [CreatedAt]");
        });

        // ═════════════════════════════════════════════════════════
        //  Key
        // ═════════════════════════════════════════════════════════
        builder.HasKey(d => d.Id);

        // ═════════════════════════════════════════════════════════
        //  Scalar properties
        // ═════════════════════════════════════════════════════════
        builder.Property(d => d.DonorId)
            .IsRequired();

        builder.Property(d => d.CharityId)
            .IsRequired(false);

        builder.Property(d => d.VolunteerId)
            .IsRequired(false);

        builder.Property(d => d.CategoryId)
            .IsRequired(false);

        builder.Property(d => d.CityId)
            .IsRequired(false);

        builder.Property(d => d.Title)
            .HasMaxLength(200);

        builder.Property(d => d.Description)
            .HasMaxLength(1000);

        builder.Property(d => d.FoodType)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(d => d.QuantityDescription)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(d => d.WeightKg)
            .HasColumnType("decimal(10,2)");

        builder.Property(d => d.ExpiryDate)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(d => d.PickupLocation)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(d => d.ImageUrl)
            .HasMaxLength(500);

        builder.Property(d => d.Status)
            .HasConversion<string>()
            .HasColumnType("tinyint")
            .IsRequired();

        builder.Property(d => d.CreatedAt)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(d => d.DeliveredAtUtc)
            .HasColumnType("datetime2");

        builder.Property(d => d.CancelledAtUtc)
            .HasColumnType("datetime2");

        builder.Property(d => d.ExpiredAtUtc)
            .HasColumnType("datetime2");

        builder.Property(d => d.CancellationReason)
            .HasMaxLength(500);

        builder.Property(d => d.IsDeleted)
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(d => d.DeletedAt)
            .HasColumnType("datetime2");

        builder.Property(d => d.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        // ═════════════════════════════════════════════════════════
        //  Backing field
        // ═════════════════════════════════════════════════════════
        builder.Navigation(d => d.DeliveryMissions)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // ═════════════════════════════════════════════════════════
        //  Relationships
        // ═════════════════════════════════════════════════════════

        // ── Donation → Donor (Account) (Restrict) ───────────────
        builder.HasOne(d => d.Donor)
            .WithMany(a => a.DonationsMade)
            .HasForeignKey(d => d.DonorId)
            .OnDelete(DeleteBehavior.Restrict);

        // ── Donation → Charity (Account) (Restrict) ─────────────
        builder.HasOne(d => d.Charity)
            .WithMany(a => a.DonationsReceived)
            .HasForeignKey(d => d.CharityId)
            .OnDelete(DeleteBehavior.Restrict);

        // ── Donation → Volunteer (Account) (Restrict) ───────────
        builder.HasOne(d => d.Volunteer)
            .WithMany(a => a.DonationsHandled)
            .HasForeignKey(d => d.VolunteerId)
            .OnDelete(DeleteBehavior.Restrict);

        // ── Donation → Category (Restrict) ──────────────────────
        builder.HasOne(d => d.Category)
            .WithMany(c => c.Donations)
            .HasForeignKey(d => d.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // ── Donation → City (Restrict) ──────────────────────────
        builder.HasOne(d => d.City)
            .WithMany()
            .HasForeignKey(d => d.CityId)
            .OnDelete(DeleteBehavior.Restrict);

        // ── Donation → DeliveryMissions (Cascade) ───────────────
        builder.HasMany(d => d.DeliveryMissions)
            .WithOne(m => m.Donation)
            .HasForeignKey(m => m.DonationId)
            .OnDelete(DeleteBehavior.Cascade);

        // ═════════════════════════════════════════════════════════
        //  Indexes
        // ═════════════════════════════════════════════════════════
        builder.HasIndex(d => d.CreatedAt)
            .HasDatabaseName("IX_Donations_CreatedAt");

        builder.HasIndex(d => d.ExpiryDate)
            .HasDatabaseName("IX_Donations_ExpiryDate");

        builder.HasIndex(d => d.Status)
            .HasDatabaseName("IX_Donations_Status");

        builder.HasIndex(d => d.CategoryId)
            .HasDatabaseName("IX_Donations_CategoryId");

        builder.HasIndex(d => d.CityId)
            .HasDatabaseName("IX_Donations_CityId");

        builder.HasIndex(d => d.DonorId)
            .HasDatabaseName("IX_Donations_DonorId");

        builder.HasIndex(d => d.CharityId)
            .HasDatabaseName("IX_Donations_CharityId");

        builder.HasIndex(d => d.VolunteerId)
            .HasDatabaseName("IX_Donations_VolunteerId");

        // ── Composite indexes for the most common queries ───────
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