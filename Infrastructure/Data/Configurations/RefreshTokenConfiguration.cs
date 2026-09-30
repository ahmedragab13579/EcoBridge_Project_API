using EcoBridge.Domain.Constants;
using EcoBridge.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcoBridge.Infrastructure.Data.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        // ═════════════════════════════════════════════════════════
        //  Table
        // ═════════════════════════════════════════════════════════
        builder.ToTable("RefreshTokens", t =>
        {
            t.HasCheckConstraint(
                "CK_RefreshTokens_Token_NotEmpty",
                "LEN(LTRIM(RTRIM([Token]))) > 0");

            t.HasCheckConstraint(
                "CK_RefreshTokens_Expiry_After_Creation",
                "[ExpiresAtUtc] > [CreatedAtUtc]");
        });

        // ═════════════════════════════════════════════════════════
        //  Key
        // ═════════════════════════════════════════════════════════
        builder.HasKey(rt => rt.Id);

        // ═════════════════════════════════════════════════════════
        //  Scalar properties
        // ═════════════════════════════════════════════════════════
        builder.Property(rt => rt.Token)
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(rt => rt.AccountId)
            .IsRequired();

        builder.Property(rt => rt.ExpiresAtUtc)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(rt => rt.CreatedAtUtc)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(rt => rt.RevokedAtUtc)
            .HasColumnType("datetime2");

        builder.Property(rt => rt.ReplacedByToken)
            .HasMaxLength(DomainConstants.Body.MaxBodyLength);

        builder.Property(rt => rt.RevocationReason)
            .HasMaxLength(DomainConstants.Body.MaxBodyLength);

        // ═════════════════════════════════════════════════════════
        //  Relationships
        // ═════════════════════════════════════════════════════════
        builder.HasOne(rt => rt.Account)
            .WithMany(a => a.RefreshTokens)
            .HasForeignKey(rt => rt.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        // ═════════════════════════════════════════════════════════
        //  Indexes
        // ═════════════════════════════════════════════════════════
        builder.HasIndex(rt => rt.Token)
            .IsUnique()
            .HasDatabaseName("UX_RefreshTokens_Token");

        builder.HasIndex(rt => rt.AccountId)
            .HasDatabaseName("IX_RefreshTokens_AccountId");

        builder.HasIndex(rt => new { rt.AccountId, rt.RevokedAtUtc })
            .HasDatabaseName("IX_RefreshTokens_AccountId_RevokedAtUtc");

        builder.HasIndex(rt => rt.ExpiresAtUtc)
            .HasDatabaseName("IX_RefreshTokens_ExpiresAtUtc");
    }
}