using System;
using System.Threading.Tasks;
using EcoBridge.Application.BackgroundJobs;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Models;
using EcoBridge.Infrastructure.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace EcoBridge.Application.UnitTests.BackgroundJobs;

public class TransientDataCleanupJobTests
{
    private EcoBridgeDbContext CreateDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<EcoBridgeDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        return new EcoBridgeDbContext(options);
    }

    [Fact]
    public async Task ExecuteAsync_Should_Purge_Notifications_Older_Than_30_Days()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var accountId = Guid.NewGuid();

        using (var db = CreateDbContext(dbName))
        {
            var account = Account.Create("User", "user@test.com", "0100000000", "hash", UserRole.Donor);
            db.Accounts.Add(account);

            // Stale notification (35 days old)
            var staleNotification = Notification.Create(account.Id, "Old", "Old Notification", NotificationType.DonationStatusChanged);
            typeof(Notification).GetProperty(nameof(Notification.CreatedAtUtc))!
                .SetValue(staleNotification, DateTime.UtcNow.AddDays(-35));

            // Fresh notification (5 days old)
            var freshNotification = Notification.Create(account.Id, "New", "Fresh Notification", NotificationType.DonationStatusChanged);
            typeof(Notification).GetProperty(nameof(Notification.CreatedAtUtc))!
                .SetValue(freshNotification, DateTime.UtcNow.AddDays(-5));

            db.Notifications.AddRange(staleNotification, freshNotification);
            await db.SaveChangesAsync();
        }

        // Act
        using (var db = CreateDbContext(dbName))
        {
            var job = new TransientDataCleanupJob(db, NullLogger<TransientDataCleanupJob>.Instance);
            await job.ExecuteAsync();
        }

        // Assert
        using (var db = CreateDbContext(dbName))
        {
            var notifications = await db.Notifications.ToListAsync();
            notifications.Should().HaveCount(1);
            notifications[0].Title.Should().Be("New");
        }
    }

    [Fact]
    public async Task ExecuteAsync_Should_Purge_Expired_And_Revoked_RefreshTokens()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        var accountId = Guid.NewGuid();

        using (var db = CreateDbContext(dbName))
        {
            var account = Account.Create("User", "user@test.com", "0100000000", "hash", UserRole.Donor);
            db.Accounts.Add(account);

            // 1. Expired token
            var expiredToken = new RefreshToken
            {
                AccountId = account.Id,
                Token = "expired-token",
                CreatedAtUtc = DateTime.UtcNow.AddDays(-10),
                ExpiresAtUtc = DateTime.UtcNow.AddDays(-2)
            };

            // 2. Revoked token
            var revokedToken = new RefreshToken
            {
                AccountId = account.Id,
                Token = "revoked-token",
                CreatedAtUtc = DateTime.UtcNow.AddDays(-2),
                ExpiresAtUtc = DateTime.UtcNow.AddDays(5),
                RevokedAtUtc = DateTime.UtcNow.AddHours(-1)
            };

            // 3. Active valid token
            var activeToken = new RefreshToken
            {
                AccountId = account.Id,
                Token = "active-token",
                CreatedAtUtc = DateTime.UtcNow.AddDays(-1),
                ExpiresAtUtc = DateTime.UtcNow.AddDays(6)
            };

            db.RefreshTokens.AddRange(expiredToken, revokedToken, activeToken);
            await db.SaveChangesAsync();
        }

        // Act
        using (var db = CreateDbContext(dbName))
        {
            var job = new TransientDataCleanupJob(db, NullLogger<TransientDataCleanupJob>.Instance);
            await job.ExecuteAsync();
        }

        // Assert
        using (var db = CreateDbContext(dbName))
        {
            var tokens = await db.RefreshTokens.ToListAsync();
            tokens.Should().HaveCount(1);
            tokens[0].Token.Should().Be("active-token");
        }
    }
}
