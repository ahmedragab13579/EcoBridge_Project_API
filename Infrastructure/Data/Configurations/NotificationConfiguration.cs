using EcoBridge.Domain.Constants;
using EcoBridge.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcoBridge.Infrastructure.Data.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        // ═════════════════════════════════════════════════════════
        //  Table
        // ═════════════════════════════════════════════════════════
        builder.ToTable("Notifications", t =>
        {
            t.HasCheckConstraint(
                "CK_Notifications_Title_NotEmpty",
                "LEN(LTRIM(RTRIM([Title]))) > 0");

            t.HasCheckConstraint(
                "CK_Notifications_Body_NotEmpty",
                "LEN(LTRIM(RTRIM([Body]))) > 0");

            t.HasCheckConstraint(
                "CK_Notifications_ReadAt_Consistency",
                "([IsRead] = 0 AND [ReadAtUtc] IS NULL) " +
                "OR ([IsRead] = 1 AND [ReadAtUtc] IS NOT NULL)");
        });

        // ═════════════════════════════════════════════════════════
        //  Key
        // ═════════════════════════════════════════════════════════
        builder.HasKey(n => n.Id);

        // ═════════════════════════════════════════════════════════
        //  Scalar properties
        // ═════════════════════════════════════════════════════════
        builder.Property(n => n.RecipientId)
            .IsRequired();

        builder.Property(n => n.Title)
            .HasMaxLength(DomainConstants.Name.MaxNameLength)
            .IsRequired();

        builder.Property(n => n.Body)
            .HasMaxLength(DomainConstants.Body.MaxBodyLength)
            .IsRequired();

        builder.Property(n => n.Type)
            .HasConversion<string>().HasMaxLength(DomainConstants.Name.MaxCodeLength)
            .IsRequired();

        builder.Property(n => n.IsRead)
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(n => n.CreatedAtUtc)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(n => n.ReadAtUtc)
            .HasColumnType("datetime2");

        builder.Property(n => n.RelatedEntityId)
            .IsRequired(false);

        // ═════════════════════════════════════════════════════════
        //  Relationships
        // ═════════════════════════════════════════════════════════
        builder.HasOne(n => n.Recipient)
            .WithMany(a => a.Notifications)
            .HasForeignKey(n => n.RecipientId)
            .OnDelete(DeleteBehavior.NoAction);

        // ═════════════════════════════════════════════════════════
        //  Indexes
        // ═════════════════════════════════════════════════════════
        builder.HasIndex(n => n.RecipientId)
            .HasDatabaseName("IX_Notifications_RecipientId");

        builder.HasIndex(n => n.CreatedAtUtc)
            .HasDatabaseName("IX_Notifications_CreatedAtUtc");

        builder.HasIndex(n => new { n.RecipientId, n.IsRead })
            .HasDatabaseName("IX_Notifications_RecipientId_IsRead");

        builder.HasIndex(n => new { n.RecipientId, n.IsRead, n.CreatedAtUtc })
            .HasDatabaseName("IX_Notifications_RecipientId_IsRead_CreatedAtUtc");

        builder.HasIndex(n => n.RelatedEntityId)
            .HasDatabaseName("IX_Notifications_RelatedEntityId");
    }
}