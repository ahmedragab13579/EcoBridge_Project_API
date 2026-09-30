using EcoBridge.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcoBridge.Infrastructure.Data.Configurations;

public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.EventType)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(m => m.Content)
            .IsRequired();

        builder.Property(m => m.OccurredOnUtc)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(m => m.ProcessedOnUtc)
            .HasColumnType("datetime2")
            .IsRequired(false);

        builder.Property(m => m.NextRetryOnUtc)
            .HasColumnType("datetime2")
            .IsRequired(false);

        builder.Property(m => m.Error)
            .HasMaxLength(4000)
            .IsRequired(false);

        builder.Property(m => m.RetryCount)
            .IsRequired();

        // Index for the background processor querying unprocessed / scheduled retry items
        builder.HasIndex(m => new { m.ProcessedOnUtc, m.NextRetryOnUtc })
            .HasDatabaseName("IX_OutboxMessages_ProcessedOnUtc_NextRetryOnUtc");

        builder.HasIndex(m => m.OccurredOnUtc)
            .HasDatabaseName("IX_OutboxMessages_OccurredOnUtc");
    }
}
