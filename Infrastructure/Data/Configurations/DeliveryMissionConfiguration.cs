using EcoBridge.Domain.Models.Mission;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcoBridge.Infrastructure.Data.Configurations;

public class DeliveryMissionConfiguration : IEntityTypeConfiguration<DeliveryMission>
{
    public void Configure(EntityTypeBuilder<DeliveryMission> builder)
    {
        // ═════════════════════════════════════════════════════════
        //  Table
        // ═════════════════════════════════════════════════════════
        builder.ToTable("DeliveryMissions", t =>
        {
            t.HasCheckConstraint(
                "CK_DeliveryMissions_PickupLocation_NotEmpty",
                "LEN(LTRIM(RTRIM([PickupLocation]))) > 0");

            t.HasCheckConstraint(
                "CK_DeliveryMissions_DropOffLocation_NotEmpty",
                "LEN(LTRIM(RTRIM([DropOffLocation]))) > 0");

            t.HasCheckConstraint(
                "CK_DeliveryMissions_OtpAttemptCount_Range",
                "[OtpAttemptCount] >= 0 AND [OtpAttemptCount] <= 5");
        });

        // ═════════════════════════════════════════════════════════
        //  Key
        // ═════════════════════════════════════════════════════════
        builder.HasKey(m => m.Id);

        // ═════════════════════════════════════════════════════════
        //  Scalar properties
        // ═════════════════════════════════════════════════════════
        builder.Property(m => m.DonationId)
            .IsRequired();

        builder.Property(m => m.VolunteerId)
            .IsRequired();

        builder.Property(m => m.PickupLocation)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(m => m.DropOffLocation)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(m => m.PickupTime)
            .HasColumnType("datetime2");

        builder.Property(m => m.DeliveryTime)
            .HasColumnType("datetime2");

        builder.Property(m => m.OtpCode)
            .HasMaxLength(6);

        builder.Property(m => m.OtpExpiresAtUtc)
            .HasColumnType("datetime2");

        builder.Property(m => m.OtpAttemptCount)
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(m => m.HangfireJobId)
            .HasMaxLength(100);

        builder.Property(m => m.CreatedAt)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(m => m.CancelledAtUtc)
            .HasColumnType("datetime2");

        builder.Property(m => m.CancellationReason)
            .HasMaxLength(500);

        builder.Property(m => m.IsDeleted)
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(m => m.DeletedAt)
            .HasColumnType("datetime2");

        builder.Property(m => m.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        // ═════════════════════════════════════════════════════════
        //  Backing field
        // ═════════════════════════════════════════════════════════
        builder.Navigation(m => m.Reviews)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // ═════════════════════════════════════════════════════════
        //  Relationships
        // ═════════════════════════════════════════════════════════

        // ── DeliveryMission → Donation (Restrict) ───────────────
        builder.HasOne(m => m.Donation)
            .WithMany(d => d.DeliveryMissions)
            .HasForeignKey(m => m.DonationId)
            .OnDelete(DeleteBehavior.Restrict);

        // ── DeliveryMission → Volunteer (Account) (Restrict) ────
        builder.HasOne(m => m.Volunteer)
            .WithMany(a => a.DeliveryMissions)
            .HasForeignKey(m => m.VolunteerId)
            .OnDelete(DeleteBehavior.Restrict);

        // ── DeliveryMission → Reviews (Restrict) ────────────────
        // Configured from the Review side.

        // ═════════════════════════════════════════════════════════
        //  Indexes
        // ═════════════════════════════════════════════════════════
        builder.HasIndex(m => m.DonationId)
            .IsUnique()
            .HasDatabaseName("UX_DeliveryMissions_DonationId");

        builder.HasIndex(m => m.VolunteerId)
            .HasDatabaseName("IX_DeliveryMissions_VolunteerId");

        builder.HasIndex(m => m.CreatedAt)
            .HasDatabaseName("IX_DeliveryMissions_CreatedAt");

        builder.HasIndex(m => new { m.DonationId, m.DeliveryTime })
            .HasDatabaseName("IX_DeliveryMissions_DonationId_DeliveryTime");

        builder.HasIndex(m => new { m.VolunteerId, m.DeliveryTime })
            .HasDatabaseName("IX_DeliveryMissions_VolunteerId_DeliveryTime");
    }
}