using System;
using System.Threading.Tasks;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Models.Accounts;
using EcoBridge.Domain.Models.Donation;
using EcoBridge.IntegrationTests.Common;
using EcoBridge.IntegrationTests.Fixtures;
using EcoBridge.IntegrationTests.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EcoBridge.IntegrationTests.DataAccess;

public class SoftDeleteAndQueryFilterTests(CustomWebApplicationFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task DbContext_Remove_Should_Perform_Soft_Delete_On_Account()
    {
        var accountId = Guid.Empty;
        await ExecuteDbContextAsync(async db =>
        {
            var account = Account.Create("Soft Delete User", $"softdelete_{Guid.NewGuid()}@ecobridge.org", "010", TestPasswordHelper.Hash(), UserRole.Donor);
            db.Accounts.Add(account);
            await db.SaveChangesAsync();
            accountId = account.Id;
        });

        await ExecuteDbContextAsync(async db =>
        {
            var account = await db.Accounts.FindAsync(accountId);
            db.Accounts.Remove(account!);
            await db.SaveChangesAsync();
        });

        await ExecuteDbContextAsync(async db =>
        {
            (await db.Accounts.FirstOrDefaultAsync(x => x.Id == accountId)).Should().BeNull();
            var deleted = await db.Accounts.IgnoreQueryFilters().SingleAsync(x => x.Id == accountId);
            deleted.IsDeleted.Should().BeTrue();
            deleted.IsActive.Should().BeFalse();
            deleted.DeletedAt.Should().NotBeNull();
        });
    }

    [Fact]
    public async Task FilteredUniqueIndex_Should_Allow_ReRegistration_After_Soft_Delete()
    {
        var email = $"reregister_{Guid.NewGuid()}@ecobridge.org";
        var firstId = Guid.Empty;
        var secondId = Guid.Empty;

        await ExecuteDbContextAsync(async db =>
        {
            var account = Account.Create("Original Owner", email, "01011111111", TestPasswordHelper.Hash(), UserRole.Donor);
            db.Accounts.Add(account);
            await db.SaveChangesAsync();
            firstId = account.Id;
            account.SoftDelete();
            await db.SaveChangesAsync();
        });

        await ExecuteDbContextAsync(async db =>
        {
            var account = Account.Create("New Owner", email, "01022222222", TestPasswordHelper.Hash(), UserRole.Donor);
            db.Accounts.Add(account);
            await db.SaveChangesAsync();
            secondId = account.Id;
        });

        await ExecuteDbContextAsync(async db =>
        {
            var active = await db.Accounts.SingleAsync(x => x.Email == email);
            active.Id.Should().Be(secondId);
            var all = await db.Accounts.IgnoreQueryFilters().Where(x => x.Email == email).ToListAsync();
            all.Should().ContainSingle(x => x.Id == firstId && x.IsDeleted);
            all.Should().ContainSingle(x => x.Id == secondId && !x.IsDeleted);
        });
    }

    [Fact]
    public async Task Donation_Query_Filter_Should_Exclude_Soft_Deleted_Donation()
    {
        var donationId = Guid.Empty;
        await ExecuteDbContextAsync(async db =>
        {
            var donor = Account.Create("Donor", $"donor_{Guid.NewGuid()}@test.com", "010", TestPasswordHelper.Hash(), UserRole.Donor);
            var donation = Donation.Create(donor.Id, "Apples", "50 kg", DateTime.UtcNow.AddDays(2), "Warehouse 3", 1, "Apples", "Fresh apples", 1);
            db.Accounts.Add(donor);
            db.Donations.Add(donation);
            await db.SaveChangesAsync();
            donationId = donation.Id;
            donation.SoftDelete();
            await db.SaveChangesAsync();
        });

        await ExecuteDbContextAsync(async db =>
        {
            (await db.Donations.FirstOrDefaultAsync(x => x.Id == donationId)).Should().BeNull();
            (await db.Donations.IgnoreQueryFilters().SingleAsync(x => x.Id == donationId)).IsDeleted.Should().BeTrue();
        });
    }
}
