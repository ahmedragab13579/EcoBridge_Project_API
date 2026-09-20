using System;
using System.Threading.Tasks;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Models;
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
        // Arrange - Create and persist account
        Guid accountId = Guid.Empty;
        var email = $"softdelete_{Guid.NewGuid()}@ecobridge.org";

        await ExecuteDbContextAsync(async db =>
        {
            var account = Account.Create("Soft Delete User", email, "010", TestPasswordHelper.Hash(), UserRole.Donor);
            db.Accounts.Add(account);
            await db.SaveChangesAsync();
            accountId = account.Id;
        });

        // Act - Call Remove on DbContext
        await ExecuteDbContextAsync(async db =>
        {
            var account = await db.Accounts.FindAsync(accountId);
            account.Should().NotBeNull();
            db.Accounts.Remove(account!);
            await db.SaveChangesAsync();
        });

        // Assert - Standard query should filter out soft-deleted entity
        await ExecuteDbContextAsync(async db =>
        {
            var queriedAccount = await db.Accounts.FirstOrDefaultAsync(a => a.Id == accountId);
            queriedAccount.Should().BeNull();

            // IgnoreQueryFilters should still find the soft-deleted entity with correct metadata
            var ignoredFilterAccount = await db.Accounts.IgnoreQueryFilters().FirstOrDefaultAsync(a => a.Id == accountId);
            ignoredFilterAccount.Should().NotBeNull();
            ignoredFilterAccount!.IsDeleted.Should().BeTrue();
            ignoredFilterAccount.DeletedAt.Should().NotBeNull();
            ignoredFilterAccount.IsActive.Should().BeFalse();
        });
    }

    [Fact]
    public async Task FilteredUniqueIndex_Should_Allow_ReRegistration_With_Same_Email_After_SoftDelete()
    {
        // Arrange - Create and then soft delete an account
        var sharedEmail = $"reregister_{Guid.NewGuid()}@ecobridge.org";
        Guid firstAccountId = Guid.Empty;
        Guid secondAccountId = Guid.Empty;

        await ExecuteDbContextAsync(async db =>
        {
            var account1 = Account.Create("Original Owner", sharedEmail, "01011111111", TestPasswordHelper.Hash(), UserRole.Donor);
            db.Accounts.Add(account1);
            await db.SaveChangesAsync();
            firstAccountId = account1.Id;

            // Soft delete account 1
            account1.SoftDelete();
            await db.SaveChangesAsync();
        });

        // Act - Register new account with identical email
        await ExecuteDbContextAsync(async db =>
        {
            var account2 = Account.Create("New Owner", sharedEmail, "01022222222", TestPasswordHelper.Hash(), UserRole.Charity);
            db.Accounts.Add(account2);
            await db.SaveChangesAsync();
            secondAccountId = account2.Id;
        });

        // Assert - Both accounts exist in DB, but only the active one is retrieved by standard queries
        await ExecuteDbContextAsync(async db =>
        {
            var activeAccount = await db.Accounts.FirstOrDefaultAsync(a => a.Email == sharedEmail);
            activeAccount.Should().NotBeNull();
            activeAccount!.Id.Should().Be(secondAccountId);
            activeAccount.FullName.Should().Be("New Owner");
            activeAccount.IsDeleted.Should().BeFalse();

            var allAccounts = await db.Accounts.IgnoreQueryFilters().Where(a => a.Email == sharedEmail).ToListAsync();
            allAccounts.Should().HaveCount(2);
            allAccounts.Should().ContainSingle(a => a.Id == firstAccountId && a.IsDeleted);
            allAccounts.Should().ContainSingle(a => a.Id == secondAccountId && !a.IsDeleted);
        });
    }

    [Fact]
    public async Task Donation_And_Review_Should_Obey_Global_Query_Filters()
    {
        // Arrange
        Guid donationId = Guid.Empty;
        int reviewId = 0;

        await ExecuteDbContextAsync(async db =>
        {
            var donorAccount = Account.Create("Donor", $"d_{Guid.NewGuid()}@test.com", "010", TestPasswordHelper.Hash(), UserRole.Donor);
            var charityAccount = Account.Create("Charity", $"c_{Guid.NewGuid()}@test.com", "010", TestPasswordHelper.Hash(), UserRole.Charity);
            var donor = Donor.Create(donorAccount.Id, UserType.Individual);
            var charity = Charity.Create(charityAccount.Id, "Charity Org", "REG-888");

            var donation = Donation.Create(donorAccount.Id, "Apples", "50 kg", DateTime.UtcNow.AddDays(2), "Warehouse 3");
            var reviewReason = new ReviewReason { Code = "EXCELLENT", FromRole = UserRole.Donor, ToRole = UserRole.Charity, IsPositive = true };

            db.Accounts.AddRange(donorAccount, charityAccount);
            db.Donors.Add(donor);
            db.Charities.Add(charity);
            db.Donations.Add(donation);
            db.ReviewReasons.Add(reviewReason);
            await db.SaveChangesAsync();

            var review = Review.Create(donorAccount.Id, charityAccount.Id, reviewReason.Id, "Great charity partner");
            db.Reviews.Add(review);
            await db.SaveChangesAsync();

            donationId = donation.Id;
            reviewId = review.Id;
        });

        // Act - Soft delete donation and review
        await ExecuteDbContextAsync(async db =>
        {
            var donation = await db.Donations.FindAsync(donationId);
            var review = await db.Reviews.FindAsync(reviewId);

            donation!.SoftDelete();
            review!.SoftDelete();
            await db.SaveChangesAsync();
        });

        // Assert - Queries exclude soft deleted entities
        await ExecuteDbContextAsync(async db =>
        {
            var queriedDonation = await db.Donations.FirstOrDefaultAsync(d => d.Id == donationId);
            var queriedReview = await db.Reviews.FirstOrDefaultAsync(r => r.Id == reviewId);

            queriedDonation.Should().BeNull();
            queriedReview.Should().BeNull();

            var ignoredDonation = await db.Donations.IgnoreQueryFilters().FirstOrDefaultAsync(d => d.Id == donationId);
            var ignoredReview = await db.Reviews.IgnoreQueryFilters().FirstOrDefaultAsync(r => r.Id == reviewId);

            ignoredDonation.Should().NotBeNull();
            ignoredDonation!.IsDeleted.Should().BeTrue();
            ignoredDonation.DeletedAt.Should().NotBeNull();

            ignoredReview.Should().NotBeNull();
            ignoredReview!.IsDeleted.Should().BeTrue();
            ignoredReview.DeletedAt.Should().NotBeNull();
        });
    }
}
