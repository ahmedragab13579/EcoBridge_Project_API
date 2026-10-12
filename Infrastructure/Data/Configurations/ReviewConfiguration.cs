using EcoBridge.Domain.Constants;
using EcoBridge.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcoBridge.Infrastructure.Data.Configurations;

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        // ═════════════════════════════════════════════════════════
        //  Table
        // ═════════════════════════════════════════════════════════
        builder.ToTable("Reviews", t =>
        {
            t.HasCheckConstraint(
                "CK_Reviews_Reviewer_Not_Reviewee",
                "[ReviewerId] <> [RevieweeId]");

            t.HasCheckConstraint(
                "CK_Reviews_Comment_NotEmpty_When_Present",
                "[Comment] IS NULL OR LEN(LTRIM(RTRIM([Comment]))) > 0");
        });

        // ═════════════════════════════════════════════════════════
        //  Key
        // ═════════════════════════════════════════════════════════
        builder.HasKey(r => r.Id);

        // ═════════════════════════════════════════════════════════
        //  Scalar properties
        // ═════════════════════════════════════════════════════════
        builder.Property(r => r.ReviewerId)
            .IsRequired();

        builder.Property(r => r.RevieweeId)
            .IsRequired();

        builder.Property(r => r.ReviewReasonId)
            .IsRequired();

        builder.Property(r => r.MissionId)
            .IsRequired();

        builder.Property(r => r.Comment)
            .HasMaxLength(DomainConstants.Body.MaxBodyLength);

        builder.Property(r => r.CreatedAt)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(r => r.IsDeleted)
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(r => r.DeletedAt)
            .HasColumnType("datetime2");

        // ═════════════════════════════════════════════════════════
        //  Relationships
        // ═════════════════════════════════════════════════════════

        // ── Review → Reviewer (Account) (NoAction) ──────────────
        builder.HasOne(r => r.Reviewer)
            .WithMany(a => a.ReviewsGiven)
            .HasForeignKey(r => r.ReviewerId)
            .OnDelete(DeleteBehavior.NoAction);

        // ── Review → Reviewee (Account) (NoAction) ──────────────
        builder.HasOne(r => r.Reviewee)
            .WithMany(a => a.ReviewsReceived)
            .HasForeignKey(r => r.RevieweeId)
            .OnDelete(DeleteBehavior.NoAction);

        // ── Review → ReviewReason (NoAction) ────────────────────
        builder.HasOne(r => r.ReviewReason)
            .WithMany(rr => rr.Reviews)
            .HasForeignKey(r => r.ReviewReasonId)
            .OnDelete(DeleteBehavior.NoAction);

        // ── Review → Mission (NoAction) ─────────────────────────
        builder.HasOne(r => r.DeliveryMission)
            .WithMany()
            .HasForeignKey(r => r.MissionId)
            .OnDelete(DeleteBehavior.NoAction);

        // ═════════════════════════════════════════════════════════
        //  Indexes
        // ═════════════════════════════════════════════════════════
        builder.HasIndex(r => r.ReviewerId)
            .HasDatabaseName("IX_Reviews_ReviewerId");

        builder.HasIndex(r => r.RevieweeId)
            .HasDatabaseName("IX_Reviews_RevieweeId");

        builder.HasIndex(r => r.ReviewReasonId)
            .HasDatabaseName("IX_Reviews_ReviewReasonId");

        builder.HasIndex(r => r.MissionId)
            .HasDatabaseName("IX_Reviews_MissionId");

        builder.HasIndex(r => r.CreatedAt)
            .HasDatabaseName("IX_Reviews_CreatedAt");

        builder.HasIndex(r => new { r.RevieweeId, r.CreatedAt })
            .HasDatabaseName("IX_Reviews_RevieweeId_CreatedAt");

        // ── Unique: one review per (reviewer, mission) ──────────
        builder.HasIndex(r => new { r.ReviewerId, r.MissionId })
            .IsUnique()
            .HasDatabaseName("UX_Reviews_ReviewerId_MissionId");
    }
}