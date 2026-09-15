using EcoBridge.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcoBridge.Infrastructure.Data.Configurations;

public class DeliveryMissionConfiguration : IEntityTypeConfiguration<DeliveryMission>
{
    public void Configure(EntityTypeBuilder<DeliveryMission> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.MissionStatus).HasColumnType("tinyint").IsRequired();
        builder.Property(x => x.PickupLocation).HasColumnType("nvarchar(255)").IsRequired();
        builder.Property(x => x.DropOffLocation).HasColumnType("nvarchar(255)").IsRequired();
        builder.Property(x => x.PickupTime).HasColumnType("datetime2");
        builder.Property(x => x.DeliveryTime).HasColumnType("datetime2");
        builder.Property(x => x.OtpCode).HasColumnType("nvarchar(10)");
        builder.Property(x => x.OtpExpiresAtUtc).HasColumnType("datetime2");
        builder.Property(x => x.HangfireJobId).HasColumnType("nvarchar(100)");
        builder.Property(x => x.CreatedAt).HasColumnType("datetime2").IsRequired();

        builder.HasOne(x => x.Volunteer)
            .WithMany(x => x.DeliveryMissions)
            .HasForeignKey(x => x.VolunteerId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(x => x.DonationRequestId).IsUnique();
        builder.HasIndex(x => x.VolunteerId);
        builder.HasIndex(x => x.MissionStatus);
        builder.HasIndex(x => x.CreatedAt);
    }
}
