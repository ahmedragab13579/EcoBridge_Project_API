using System;
using System.Threading.Tasks;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Models;
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
    public async Task Concurrency_Should_Detect_Conflict_When_Two_Contexts_Update_Simultaneously()
    {
        // Arrange - Seed donation
        var donorId = Guid.NewGuid();
        Guid donationId = Guid.Empty;
        Guid charityId = Guid.Empty;

        await ExecuteDbContextAsync(async db =>
        {
            // 1. إنشاء حساب الجمعية وإضافته
            var charityAccount = Account.Create("Charity Concurrency", $"charity_{Guid.NewGuid()}@test.com", "01000", TestPasswordHelper.Hash(), UserRole.Charity);
            var charity = Charity.Create(charityAccount.Id, "01000", "hamed");
            charityId = charityAccount.Id;

            // 2. إنشاء حساب المتبرع وإضافته
            var donorAccount = Account.Create("Donor Concurrency", $"donor_{Guid.NewGuid()}@test.com", "01000", TestPasswordHelper.Hash(), UserRole.Donor);
            var donor = Donor.Create(donorAccount.Id, UserType.Individual);

            // 3. إنشاء التبرع
            var donation = Donation.Create(donorAccount.Id, "Fresh Apples", "10 kg", DateTime.UtcNow.AddDays(2), "Warehouse 1");

            // 4. إضافة كل الكيانات للقاعدة
            db.Accounts.Add(charityAccount);
            db.Charities.Add(charity);
            db.Accounts.Add(donorAccount);
            db.Donors.Add(donor);
            db.Donations.Add(donation);

            await db.SaveChangesAsync();

            donationId = donation.Id;
        });

        // Act - Load donation in two separate DbContext instances
        using var scope1 = Factory.Services.CreateScope();
        var db1 = scope1.ServiceProvider.GetRequiredService<EcoBridgeDbContext>();
        var donationFromDb1 = await db1.Donations.FindAsync(donationId);

        using var scope2 = Factory.Services.CreateScope();
        var db2 = scope2.ServiceProvider.GetRequiredService<EcoBridgeDbContext>();
        var donationFromDb2 = await db2.Donations.FindAsync(donationId);

        // First context modifies and saves
        donationFromDb1!.MarkAsAccepted(charityId);
        await db1.SaveChangesAsync();

        // Second context tries to modify stale entity and save
        donationFromDb2!.MarkAsAccepted(charityId);

        // Assert - db2 save should throw DbUpdateConcurrencyException due to RowVersion mismatch
        Func<Task> act = async () => await db2.SaveChangesAsync();
        await act.Should().ThrowAsync<DbUpdateConcurrencyException>();
    }
}
