using System;
using System.Threading.Tasks;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Models;
using EcoBridge.Domain.Models.Accounts;
using EcoBridge.Domain.Models.Donation;
using EcoBridge.Infrastructure.Data;
using EcoBridge.IntegrationTests.Common;
using EcoBridge.IntegrationTests.Fixtures;
using EcoBridge.IntegrationTests.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EcoBridge.IntegrationTests.DataAccess;

public class DonationConcurrencyTests(CustomWebApplicationFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task Concurrent_Donation_Updates_Should_Use_Row_Version()
    {
        var donationId = Guid.Empty;

        // 1. Prepare Seed Data
        await ExecuteDbContextAsync(async db =>
        {
            var category = Category.Create("طعام", "Food");
            var donor = Account.Create("Donor", $"donor_{Guid.NewGuid()}@test.com", "010", TestPasswordHelper.Hash(), UserRole.Donor);

            db.Categories.Add(category);
            db.Accounts.Add(donor);

            await db.SaveChangesAsync();

            var donation = Donation.Create(
                donor.Id,
                "Fresh Apples",
                "10 kg",
                DateTime.UtcNow.AddDays(2),
                "Warehouse 1",
                1,
                "Apples",
                "Fresh apples",
                category.Id);

            db.Donations.Add(donation);
            await db.SaveChangesAsync();

            donationId = donation.Id;
        });

        // 2. Simulate Concurrent Transactions
        using var scope1 = Factory.Services.CreateScope();
        using var scope2 = Factory.Services.CreateScope();

        var db1 = scope1.ServiceProvider.GetRequiredService<EcoBridgeDbContext>();
        var db2 = scope2.ServiceProvider.GetRequiredService<EcoBridgeDbContext>();

        var first = await db1.Donations.SingleAsync(x => x.Id == donationId);
        var second = await db2.Donations.SingleAsync(x => x.Id == donationId);

        // 3. First update succeeds and changes the RowVersion/Timestamp
        first.UpdateDetails("Updated one", "Description one", "Fruit", "10 kg", null, DateTime.UtcNow.AddDays(3), "Warehouse 1", 1, 1);
        await db1.SaveChangesAsync();

        // 4. Second update attempts to save with stale RowVersion
        second.UpdateDetails("Updated two", "Description two", "Fruit", "10 kg", null, DateTime.UtcNow.AddDays(3), "Warehouse 1", 1, 1);

        // 5. Assert concurrency exception is thrown
        Func<Task> action = () => db2.SaveChangesAsync();
        await action.Should().ThrowAsync<DbUpdateConcurrencyException>();
    }
}