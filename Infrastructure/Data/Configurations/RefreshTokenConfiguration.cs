using EcoBridge.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcoBridge.Infrastructure.Data.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Token).HasColumnType("nvarchar(512)").IsRequired();
        builder.Property(x => x.ExpiresAtUtc).HasColumnType("datetime2").IsRequired();
        builder.Property(x => x.CreatedAtUtc).HasColumnType("datetime2").IsRequired();
        builder.Property(x => x.RevokedAtUtc).HasColumnType("datetime2");
        builder.Property(x => x.ReplacedByToken).HasColumnType("nvarchar(512)");

        builder.HasIndex(x => x.Token).IsUnique();
        builder.HasIndex(x => x.AccountId);
    }
}
