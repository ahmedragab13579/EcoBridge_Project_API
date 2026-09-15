using EcoBridge.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcoBridge.Infrastructure.Data.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title).HasColumnType("nvarchar(200)").IsRequired();
        builder.Property(x => x.Body).HasColumnType("nvarchar(1000)").IsRequired();
        builder.Property(x => x.Type).HasColumnType("tinyint").IsRequired();
        builder.Property(x => x.IsRead).HasDefaultValue(false);
        builder.Property(x => x.CreatedAtUtc).HasColumnType("datetime2").IsRequired();
        builder.Property(x => x.RelatedEntityId);

        builder.HasOne(x => x.Recipient)
            .WithMany(x => x.Notifications)
            .HasForeignKey(x => x.RecipientId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes for performance (frequent query: unread notifications by user, ordered by date)
        builder.HasIndex(x => x.RecipientId);
        builder.HasIndex(x => new { x.RecipientId, x.IsRead });
        builder.HasIndex(x => x.CreatedAtUtc);
    }
}
