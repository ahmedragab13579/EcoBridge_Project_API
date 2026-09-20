using System;
using System.Threading.Tasks;
using EcoBridge.Application.BackgroundJobs;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Models.Accounts;
using EcoBridge.Domain.Models.Donation;
using EcoBridge.E2ETests.Common;
using EcoBridge.E2ETests.Fixtures;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EcoBridge.E2ETests.Flows;

public class DonationExpiryJobE2ETests(CustomE2EWebApplicationFactory factory) : BaseE2ETest(factory)
{
    [Fact]
    public async Task DonationExpiryJob_Should_Mark_Stale_Donations_As_Expired()
    {
        // ==========================================
        // 1. Arrange - Seed 1 Stale Donation and 1 Fresh Donation
        // ==========================================
        Guid staleDonationId = Guid.Empty;
        Guid freshDonationId = Guid.Empty;

        await ExecuteDbContextAsync(async db =>
        {
            var donorAccount = Account.Create("Donor", $"d_{Guid.NewGuid()}@test.com", "010", TestPasswordHelper.Hash(), UserRole.Donor);
            var donor = Donor.Create(donorAccount.Id, UserType.Individual);

            // Stale donation (expired 2 hours ago)
            var staleDonation = Donation.Create(
                donorAccount.Id,
                "Fresh Milk",
                "5 bottles",
                DateTime.UtcNow.AddHours(-2),
                "Supermarket A"
            );

            // Fresh donation (expires in 2 days)
            var freshDonation = Donation.Create(
                donorAccount.Id,
                "Dry Pasta",
                "20 packs",
                DateTime.UtcNow.AddDays(2),
                "Supermarket A"
            );

            db.Accounts.Add(donorAccount);
            db.Donors.Add(donor);
            db.Donations.AddRange(staleDonation, freshDonation);
            await db.SaveChangesAsync();

            staleDonationId = staleDonation.Id;
            freshDonationId = freshDonation.Id;
        });

        // ==========================================
        // 2. Act - Execute DonationExpiryJob
        // ==========================================
        using (var scope = Factory.Services.CreateScope())
        {
            var expiryJob = scope.ServiceProvider.GetRequiredService<DonationExpiryJob>();
            await expiryJob.ExecuteAsync();
        }

        // ==========================================
        // 3. Assert - Verify Status Updates
        // ==========================================
        await ExecuteDbContextAsync(async db =>
        {
            var updatedStale = await db.Donations.FindAsync(staleDonationId);
            updatedStale.Should().NotBeNull();
            updatedStale!.Status.Should().Be(DonationStatus.Expired);

            var updatedFresh = await db.Donations.FindAsync(freshDonationId);
            updatedFresh.Should().NotBeNull();
            updatedFresh!.Status.Should().Be(DonationStatus.Available);
        });
    }
}
