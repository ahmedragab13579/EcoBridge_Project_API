using EcoBridge.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcoBridge.Infrastructure.Data.Configurations;

public class PenaltyLogConfiguration : IEntityTypeConfiguration<PenaltyLog>
{
    public void Configure(EntityTypeBuilder<PenaltyLog> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Amount).IsRequired();
        builder.Property(x => x.ReasonCode).HasMaxLength(100).IsRequired();
        builder.Property(x => x.CreatedAtUtc).HasColumnType("datetime2").IsRequired();

        builder.HasOne(x => x.Account)
            .WithMany(x => x.PenaltyLogs)
            .HasForeignKey(x => x.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Mission)
            .WithMany()
            .HasForeignKey(x => x.MissionId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.HasIndex(x => x.AccountId);
        builder.HasIndex(x => x.CreatedAtUtc);
        builder.HasIndex(x => x.ReasonCode);
    }
}
