using EcoBridge.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcoBridge.Infrastructure.Data.Configurations;

public class PointTransactionConfiguration : IEntityTypeConfiguration<PointTransaction>
{
    public void Configure(EntityTypeBuilder<PointTransaction> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Amount).IsRequired();
        builder.Property(x => x.TransactionType).HasColumnType("tinyint").IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnType("datetime2").IsRequired();

        builder.HasOne(x => x.User)
            .WithMany(x => x.PointTransactions)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.CreatedAt);
        builder.HasIndex(x => x.TransactionType);
    }
}
