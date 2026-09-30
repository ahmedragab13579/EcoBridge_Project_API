using EcoBridge.Domain.Constants;
using EcoBridge.Domain.Models.Mission;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcoBridge.Infrastructure.Data.Configurations;

public class DeliveryMissionConfiguration : IEntityTypeConfiguration<DeliveryMission>
{
    public void Configure(EntityTypeBuilder<DeliveryMission> builder)
    {
        // ═════════════════════════════════════════════════════════
        //  Table + Check Constraints
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

        builder.HasKey(m => m.Id);

        // ═════════════════════════════════════════════════════════
        //  Scalar properties
        // ═════════════════════════════════════════════════════════
        builder.Property(m => m.DonationId)
            .IsRequired();

        builder.Property(m => m.VolunteerId)
            .IsRequired(false);   // خليها false لو بيتعين بعدين

        builder.Property(m => m.PickupLocation)
            .HasMaxLength(DomainConstants.Body.MaxLocationLength)
            .IsRequired();

        builder.Property(m => m.DropOffLocation)
            .HasMaxLength(DomainConstants.Body.MaxLocationLength)
            .IsRequired();

        builder.Property(m => m.PickupTime).HasColumnType("datetime2");
        builder.Property(m => m.DeliveryTime).HasColumnType("datetime2");

        builder.Property(m => m.OtpCode).HasMaxLength(DomainConstants.MissionVal.OtpLength);
        builder.Property(m => m.OtpExpiresAtUtc).HasColumnType("datetime2");

        builder.Property(m => m.OtpAttemptCount)
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(m => m.HangfireJobId).HasMaxLength(DomainConstants.Body.MaxJobLength);

        builder.Property(m => m.CreatedAt)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(m => m.CancelledAtUtc).HasColumnType("datetime2");
        builder.Property(m => m.CancellationReason).HasMaxLength(DomainConstants.Body.MaxBodyLength);

        // ✅ enum string + IsRequired
        builder.Property(m => m.Status)
            .HasConversion<string>()
            .HasMaxLength(DomainConstants.Name.MaxCodeLength)
            .IsRequired();

        builder.Property(m => m.IsDeleted)
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(m => m.DeletedAt).HasColumnType("datetime2");

        // ✅ IsRowVersion() كفاية
        builder.Property(m => m.RowVersion)
            .IsRowVersion();


        // ═════════════════════════════════════════════════════════
        //  Relationships
        // ═════════════════════════════════════════════════════════
        builder.HasOne(m => m.Donation)
            .WithMany()
            .HasForeignKey(m => m.DonationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.Volunteer)
            .WithMany(a => a.DeliveryMissions)
            .HasForeignKey(m => m.VolunteerId)
            .OnDelete(DeleteBehavior.Restrict);

        // ═════════════════════════════════════════════════════════
        //  Indexes
        // ═════════════════════════════════════════════════════════

        // ✅ Filtered Unique: Donation واحدة ليها Mission نشطة واحدة بس
        builder.HasIndex(m => m.DonationId)
            .IsUnique()
            .HasFilter("[IsDeleted] = 0")
            .HasDatabaseName("UX_DeliveryMissions_DonationId_Active");

        builder.HasIndex(m => m.VolunteerId)
            .HasDatabaseName("IX_DeliveryMissions_VolunteerId");

        builder.HasIndex(m => m.Status)
            .HasDatabaseName("IX_DeliveryMissions_Status");

        builder.HasIndex(m => m.CreatedAt)
            .HasDatabaseName("IX_DeliveryMissions_CreatedAt");

        builder.HasIndex(m => m.OtpExpiresAtUtc)
            .HasDatabaseName("IX_DeliveryMissions_OtpExpiresAtUtc");

        builder.HasIndex(m => new { m.DonationId, m.DeliveryTime })
            .HasDatabaseName("IX_DeliveryMissions_DonationId_DeliveryTime");

        builder.HasIndex(m => new { m.VolunteerId, m.DeliveryTime })
            .HasDatabaseName("IX_DeliveryMissions_VolunteerId_DeliveryTime");

    }
}