using System;
using System.Threading.Tasks;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Models;
using EcoBridge.Domain.Models.Accounts;
using EcoBridge.Domain.Models.Donation;
using EcoBridge.Domain.Models.Mission;
using EcoBridge.IntegrationTests.Common;
using EcoBridge.IntegrationTests.Fixtures;
using EcoBridge.IntegrationTests.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EcoBridge.IntegrationTests.DataAccess;

public class EntityPersistenceTests(CustomWebApplicationFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task Account_Should_Enforce_Unique_Email_Constraint()
    {
        // Arrange
        var duplicateEmail = $"duplicate_{Guid.NewGuid()}@ecobridge.org";

        await ExecuteDbContextAsync(async db =>
        {
            var account1 = Account.Create("User One", duplicateEmail, "010111", TestPasswordHelper.Hash(), UserRole.Donor);
            db.Accounts.Add(account1);
            await db.SaveChangesAsync();
        });

        // Act & Assert - Attempting to insert duplicate email should throw DbUpdateException
        Func<Task> act = async () =>
        {
            await ExecuteDbContextAsync(async db =>
            {
                var account2 = Account.Create("User Two", duplicateEmail, "010222", TestPasswordHelper.Hash(), UserRole.Charity);
                db.Accounts.Add(account2);
                await db.SaveChangesAsync();
            });
        };

        await act.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task Geographic_Hierarchy_Should_Persist_Relationships_Correctly()
    {
        // Arrange
        int cityId = 0;
        await ExecuteDbContextAsync(async db =>
        {
            var country = new Country { NameAr = "مصر", NameEn = "Egypt", Code = $"EG_{Guid.NewGuid().ToString()[..4]}" };
            var gov = new Governorate { NameAr = "القاهرة", NameEn = "Cairo", Country = country };
            var city = new City { NameAr = "المعادي", NameEn = "Maadi", Governorate = gov };

            db.Countries.Add(country);
            db.Governorates.Add(gov);
            db.Cities.Add(city);
            await db.SaveChangesAsync();

            cityId = city.Id;
        });

        // Act & Assert
        await ExecuteDbContextAsync(async db =>
        {
            var fetchedCity = await db.Cities
                .Include(c => c.Governorate)
                .ThenInclude(g => g.Country)
                .FirstOrDefaultAsync(c => c.Id == cityId);

            fetchedCity.Should().NotBeNull();
            fetchedCity!.NameEn.Should().Be("Maadi");
            fetchedCity.Governorate.Should().NotBeNull();
            fetchedCity.Governorate.NameEn.Should().Be("Cairo");
            fetchedCity.Governorate.Country.Should().NotBeNull();
            fetchedCity.Governorate.Country.NameEn.Should().Be("Egypt");
        });
    }

    [Fact]
    public async Task Donation_And_DeliveryMission_Full_Relationship_Should_Persist()
    {
        // Arrange
        Guid missionId = Guid.Empty;

        await ExecuteDbContextAsync(async db =>
        {
            var donorAccount = Account.Create("Donor", $"donor_{Guid.NewGuid()}@test.com", "010", TestPasswordHelper.Hash(), UserRole.Donor);
            var doner = Donor.Create(donorAccount.Id, UserType.Individual);
            var charityAccount = Account.Create("Charity", $"charity_{Guid.NewGuid()}@test.com", "010", TestPasswordHelper.Hash(), UserRole.Charity);
            var charity = Charity.Create(charityAccount.Id, "Charity Org", "REG-001");

            var donation = Donation.Create(donorAccount.Id, "Oranges", "20 kg", DateTime.UtcNow.AddDays(2), "Warehouse 5");
            var donationRequest = DonationRequest.Create(donation.Id, charity.AccountId);
            donationRequest.Donation = donation;
            donationRequest.Charity = charity;

            var mission = DeliveryMission.Create(donationRequest.Id, "Pickup Spot", "Dropoff Spot", "7788", DateTime.UtcNow.AddHours(24));
            mission.DonationRequest = donationRequest;

            db.Accounts.AddRange(donorAccount, charityAccount);
            db.Donors.Add(doner);
            db.Charities.Add(charity);
            db.Donations.Add(donation);
            db.DonationRequests.Add(donationRequest);
            db.DeliveryMissions.Add(mission);
            await db.SaveChangesAsync();

            missionId = mission.Id;
        });

        // Act & Assert
        await ExecuteDbContextAsync(async db =>
        {
            var fetchedMission = await db.DeliveryMissions
                .Include(m => m.DonationRequest)
                    .ThenInclude(r => r.Donation)
                .Include(m => m.DonationRequest)
                    .ThenInclude(r => r.Charity)
                .FirstOrDefaultAsync(m => m.Id == missionId);

            fetchedMission.Should().NotBeNull();
            fetchedMission!.OtpCode.Should().Be("7788");
            fetchedMission.DonationRequest.Should().NotBeNull();
            fetchedMission.DonationRequest.Donation.Should().NotBeNull();
            fetchedMission.DonationRequest.Donation.FoodType.Should().Be("Oranges");
            fetchedMission.DonationRequest.Charity.OrganizationName.Should().Be("Charity Org");
        });
    }
}
