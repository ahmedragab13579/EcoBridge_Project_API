using EcoBridge.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcoBridge.Infrastructure.Data.Configurations;

public class DonationRequestConfiguration : IEntityTypeConfiguration<DonationRequest>
{
    public void Configure(EntityTypeBuilder<DonationRequest> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.RequestStatus).HasColumnType("tinyint").IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnType("datetime2").IsRequired();

        builder.HasOne(x => x.Donation)
            .WithMany(x => x.DonationRequests)
            .HasForeignKey(x => x.DonationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Charity)
            .WithMany(x => x.DonationRequests)
            .HasForeignKey(x => x.CharityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.DeliveryMission)
            .WithOne(x => x.DonationRequest)
            .HasForeignKey<DeliveryMission>(x => x.DonationRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.DonationId);
        builder.HasIndex(x => x.CharityId);
        builder.HasIndex(x => x.RequestStatus);
        builder.HasIndex(x => x.CreatedAt);
    }
}
