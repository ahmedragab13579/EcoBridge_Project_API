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
        var email = $"duplicate_{Guid.NewGuid()}@ecobridge.org";
        await ExecuteDbContextAsync(async db =>
        {
            db.Accounts.Add(Account.Create("User One", email, "010111", TestPasswordHelper.Hash(), UserRole.Donor));
            await db.SaveChangesAsync();
        });

        Func<Task> action = () => ExecuteDbContextAsync(async db =>
        {
            db.Accounts.Add(Account.Create("User Two", email, "010222", TestPasswordHelper.Hash(), UserRole.Donor));
            await db.SaveChangesAsync();
        });

        await action.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task Geographic_Hierarchy_Should_Persist_Relationships_Correctly()
    {
        var cityId = 0;
        var uniqueSuffix = Guid.NewGuid().ToString("N")[..6];
        var countryNameEn = $"Country_{uniqueSuffix}";
        var govNameEn = $"Gov_{uniqueSuffix}";
        var cityNameEn = $"City_{uniqueSuffix}";

        await ExecuteDbContextAsync(async db =>
        {
            var country = new Country { NameAr = $"دولة_{uniqueSuffix}", NameEn = countryNameEn, Code = $"C_{uniqueSuffix}" };
            var governorate = new Governorate { NameAr = $"محافظة_{uniqueSuffix}", Code = $"g_{uniqueSuffix}", NameEn = govNameEn, Country = country };
            var city = new City { NameAr = $"مدينة_{uniqueSuffix}", NameEn = cityNameEn, Code = $"c_{uniqueSuffix}", Governorate = governorate };
            db.Countries.Add(country);
            db.Governorates.Add(governorate);
            db.Cities.Add(city);
            await db.SaveChangesAsync();
            cityId = city.Id;
        });

        await ExecuteDbContextAsync(async db =>
        {
            var city = await db.Cities.Include(x => x.Governorate).ThenInclude(x => x.Country).SingleAsync(x => x.Id == cityId);
            city.NameEn.Should().Be(cityNameEn);
            city.Governorate.NameEn.Should().Be(govNameEn);
            city.Governorate.Country.NameEn.Should().Be(countryNameEn);
        });
    }

    [Fact]
    public async Task Donation_And_DeliveryMission_Should_Persist_Current_Relationships()
    {
        var missionId = Guid.Empty;
        await ExecuteDbContextAsync(async db =>
        {
            var donor = Account.Create("Donor", $"donor_{Guid.NewGuid()}@test.com", "010", TestPasswordHelper.Hash(), UserRole.Donor);
            var volunteer = Account.Create("Volunteer", $"volunteer_{Guid.NewGuid()}@test.com", "011", TestPasswordHelper.Hash(), UserRole.Volunteer);
            var charity = Account.Create("Charity", $"charity_{Guid.NewGuid()}@test.com", "012", TestPasswordHelper.Hash(), UserRole.Charity, UserType.Company, organizationName: "Food Bank", registrationNumber: "REG-1");
            var donation = Donation.Create(donor.Id, "Oranges", "20 kg", DateTime.UtcNow.AddDays(2), "Warehouse 5", 1, "Fresh oranges", "Food donation", 1);
            donation.Claim(charity.Id);
            donation.AssignVolunteer(volunteer.Id);
            var mission = DeliveryMission.Create(donation.Id, volunteer.Id, "Warehouse 5", "Charity destination");
            db.Accounts.AddRange(donor, volunteer, charity);
            db.Donations.Add(donation);
            db.DeliveryMissions.Add(mission);
            await db.SaveChangesAsync();
            missionId = mission.Id;
        });

        await ExecuteDbContextAsync(async db =>
        {
            var mission = await db.DeliveryMissions.Include(x => x.Donation).SingleAsync(x => x.Id == missionId);
            mission.Donation.FoodType.Should().Be("Oranges");
            mission.VolunteerId.Should().NotBeEmpty();
        });
    }
}
