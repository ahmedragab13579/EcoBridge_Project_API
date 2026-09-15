using EcoBridge.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcoBridge.Infrastructure.Data.Configurations;

public class ReviewReasonConfiguration : IEntityTypeConfiguration<ReviewReason>
{
    public void Configure(EntityTypeBuilder<ReviewReason> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code).HasColumnType("nvarchar(50)").IsRequired();
        builder.Property(x => x.FromRole).HasColumnType("tinyint").IsRequired();
        builder.Property(x => x.ToRole).HasColumnType("tinyint").IsRequired();
        builder.Property(x => x.IsPositive).IsRequired();

        builder.HasIndex(x => x.Code);
    }
}
