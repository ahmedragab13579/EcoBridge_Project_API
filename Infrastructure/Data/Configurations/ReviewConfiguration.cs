using EcoBridge.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcoBridge.Infrastructure.Data.Configurations;

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Comment).HasColumnType("nvarchar(500)");
        builder.Property(x => x.CreatedAt).HasColumnType("datetime2").IsRequired();
        builder.Property(x => x.IsDeleted).HasDefaultValue(false);
        builder.Property(x => x.DeletedAt).HasColumnType("datetime2");

        // Global Query Filter for Soft Delete
        builder.HasQueryFilter(x => !x.IsDeleted);

        builder.HasOne(x => x.Reviewer)
            .WithMany(x => x.ReviewsGiven)
            .HasForeignKey(x => x.ReviewerId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.Reviewee)
            .WithMany(x => x.ReviewsReceived)
            .HasForeignKey(x => x.RevieweeId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.ReviewReason)
            .WithMany(x => x.Reviews)
            .HasForeignKey(x => x.ReviewReasonId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Mission)
            .WithMany(x => x.Reviews)
            .HasForeignKey(x => x.MissionId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.Request)
            .WithMany(x => x.Reviews)
            .HasForeignKey(x => x.RequestId)
            .OnDelete(DeleteBehavior.NoAction);

        // Indexes
        builder.HasIndex(x => x.ReviewerId);
        builder.HasIndex(x => x.RevieweeId);
        builder.HasIndex(x => x.ReviewReasonId);
        builder.HasIndex(x => x.MissionId);
        builder.HasIndex(x => x.RequestId);
        builder.HasIndex(x => x.CreatedAt);
    }
}
