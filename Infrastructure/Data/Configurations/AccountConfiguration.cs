using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcoBridge.Infrastructure.Data.Configurations;

public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.FullName).HasColumnType("nvarchar(150)").IsRequired();
        builder.Property(x => x.Email).HasColumnType("nvarchar(255)").IsRequired();
        builder.Property(x => x.Phone).HasColumnType("nvarchar(20)").IsRequired();
        builder.Property(x => x.PasswordHash).HasColumnType("nvarchar(max)").IsRequired();
        builder.Property(x => x.ImagePath).HasColumnType("nvarchar(500)");
        builder.Property(x => x.AddressDetails).HasColumnType("nvarchar(500)");
        builder.Property(x => x.RoleId).HasColumnType("tinyint").IsRequired();
        builder.Property(x => x.UserType).HasColumnType("tinyint").IsRequired().HasDefaultValue(UserType.Individual);
        builder.Property(x => x.TotalWalletPoints).HasDefaultValue(0);
        builder.Property(x => x.TrustScore).HasDefaultValue(100);
        builder.Property(x => x.CreatedAt).HasColumnType("datetime2").IsRequired();
        builder.Property(x => x.IsActive).HasDefaultValue(true);
        builder.Property(x => x.IsDeleted).HasDefaultValue(false);
        builder.Property(x => x.DeletedAt).HasColumnType("datetime2");

        builder.HasOne(x => x.City)
            .WithMany()
            .HasForeignKey(x => x.CityId)
            .OnDelete(DeleteBehavior.SetNull);

        // Filtered Unique Indexes (Only active accounts must have unique email)
        builder.HasIndex(x => x.Email)
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        builder.HasIndex(x => x.NormalizedEmail)
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => x.NormalizedUserName).HasDatabaseName("UserNameIndex").IsUnique().HasFilter("[IsDeleted]=0");

        // Global Query Filter for Soft Delete
        builder.HasQueryFilter(x => !x.IsDeleted);

        builder.HasOne(x => x.Donor)
            .WithOne(x => x.Account)
            .HasForeignKey<Donor>(x => x.AccountId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.Charity)
            .WithOne(x => x.Account)
            .HasForeignKey<Charity>(x => x.AccountId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.Volunteer)
            .WithOne(x => x.Account)
            .HasForeignKey<Volunteer>(x => x.AccountId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.Admin)
            .WithOne(x => x.Account)
            .HasForeignKey<Admin>(x => x.AccountId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasMany(x => x.RefreshTokens)
            .WithOne(x => x.Account)
            .HasForeignKey(x => x.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.PointTransactions)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.ReviewsGiven)
            .WithOne(x => x.Reviewer)
            .HasForeignKey(x => x.ReviewerId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasMany(x => x.ReviewsReceived)
            .WithOne(x => x.Reviewee)
            .HasForeignKey(x => x.RevieweeId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
