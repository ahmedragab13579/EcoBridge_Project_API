using System;
using System.Threading.Tasks;
using EcoBridge.Application.BackgroundJobs;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Models;
using EcoBridge.E2ETests.Common;
using EcoBridge.E2ETests.Fixtures;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EcoBridge.E2ETests.Flows;

public class TransientDataCleanupJobE2ETests(CustomE2EWebApplicationFactory factory) : BaseE2ETest(factory)
{
    [Fact]
    public async Task TransientDataCleanupJob_Should_Hard_Delete_Stale_Notifications_And_Expired_Tokens()
    {
        // ==========================================
        // 1. Arrange - Seed stale & fresh notifications and refresh tokens
        // ==========================================
        int staleNotificationId = 0;
        int freshNotificationId = 0;
        int expiredTokenId = 0;
        int revokedTokenId = 0;
        int activeTokenId = 0;

        await ExecuteDbContextAsync(async db =>
        {
            var account = Account.Create("CleanupUser", $"cleanup_{Guid.NewGuid()}@test.com", "0109999999", TestPasswordHelper.Hash(), UserRole.Donor);
            db.Accounts.Add(account);
            await db.SaveChangesAsync();

            // Stale notification (>30 days old)
            var staleNotification = Notification.Create(account.Id, "Old Stale Alert", "35 days old", NotificationType.DonationStatusChanged);
            typeof(Notification).GetProperty(nameof(Notification.CreatedAtUtc))!
                .SetValue(staleNotification, DateTime.UtcNow.AddDays(-35));

            // Fresh notification (<30 days old)
            var freshNotification = Notification.Create(account.Id, "Fresh Alert", "5 days old", NotificationType.DonationStatusChanged);
            typeof(Notification).GetProperty(nameof(Notification.CreatedAtUtc))!
                .SetValue(freshNotification, DateTime.UtcNow.AddDays(-5));

            db.Notifications.AddRange(staleNotification, freshNotification);

            // Expired token
            var expiredToken = new RefreshToken
            {
                AccountId = account.Id,
                Token = $"expired_{Guid.NewGuid()}",
                CreatedAtUtc = DateTime.UtcNow.AddDays(-10),
                ExpiresAtUtc = DateTime.UtcNow.AddDays(-1)
            };

            // Revoked token
            var revokedToken = new RefreshToken
            {
                AccountId = account.Id,
                Token = $"revoked_{Guid.NewGuid()}",
                CreatedAtUtc = DateTime.UtcNow.AddDays(-2),
                ExpiresAtUtc = DateTime.UtcNow.AddDays(5),
                RevokedAtUtc = DateTime.UtcNow.AddHours(-1)
            };

            // Active valid token
            var activeToken = new RefreshToken
            {
                AccountId = account.Id,
                Token = $"active_{Guid.NewGuid()}",
                CreatedAtUtc = DateTime.UtcNow.AddDays(-1),
                ExpiresAtUtc = DateTime.UtcNow.AddDays(7)
            };

            db.RefreshTokens.AddRange(expiredToken, revokedToken, activeToken);
            await db.SaveChangesAsync();

            staleNotificationId = staleNotification.Id;
            freshNotificationId = freshNotification.Id;
            expiredTokenId = expiredToken.Id;
            revokedTokenId = revokedToken.Id;
            activeTokenId = activeToken.Id;
        });

        // ==========================================
        // 2. Act - Execute TransientDataCleanupJob
        // ==========================================
        using (var scope = Factory.Services.CreateScope())
        {
            var cleanupJob = scope.ServiceProvider.GetRequiredService<TransientDataCleanupJob>();
            await cleanupJob.ExecuteAsync();
        }

        // ==========================================
        // 3. Assert - Verify Hard Deletions
        // ==========================================
        await ExecuteDbContextAsync(async db =>
        {
            // Notifications
            var staleNotification = await db.Notifications.FindAsync(staleNotificationId);
            staleNotification.Should().BeNull("Stale notification older than 30 days must be hard deleted");

            var freshNotification = await db.Notifications.FindAsync(freshNotificationId);
            freshNotification.Should().NotBeNull("Fresh notification must be retained");

            // Refresh Tokens
            var expiredToken = await db.RefreshTokens.FindAsync(expiredTokenId);
            expiredToken.Should().BeNull("Expired refresh token must be hard deleted");

            var revokedToken = await db.RefreshTokens.FindAsync(revokedTokenId);
            revokedToken.Should().BeNull("Revoked refresh token must be hard deleted");

            var activeToken = await db.RefreshTokens.FindAsync(activeTokenId);
            activeToken.Should().NotBeNull("Active refresh token must be retained");
        });
    }
}
