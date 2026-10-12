using System;
using System.Net;
using System.Threading.Tasks;
using EcoBridge.Application.BackgroundJobs;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Models.Accounts;
using EcoBridge.Domain.Models.Donation;
using EcoBridge.Domain.Models.Mission;
using EcoBridge.E2ETests.Common;
using EcoBridge.E2ETests.Fixtures;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EcoBridge.E2ETests.Flows;

public class VolunteerTimeoutE2ETests(CustomE2EWebApplicationFactory factory) : BaseE2ETest(factory)
{
    [Fact]
    public async Task Volunteer_Timeout_Should_Apply_Penalty_And_Allow_Reassignment()
    {
        Guid donationId = Guid.Empty;
        Guid missionId = Guid.Empty;
        Guid vol1AccountId = Guid.Empty;
        Guid vol2AccountId = Guid.Empty;

        var vol1Email = $"vol1_timeout_{Guid.NewGuid()}@ecobridge.org";
        var vol2Email = $"vol2_reassigned_{Guid.NewGuid()}@ecobridge.org";

        await ExecuteDbContextAsync(async db =>
        {
            var donorAccount = Account.Create(
                "Donor",
                $"d_{Guid.NewGuid()}@test.com",
                "01012345678",
                TestPasswordHelper.Hash(),
                UserRole.Donor);

            var charityAccount = Account.Create(
                "Charity",
                $"c_{Guid.NewGuid()}@test.com",
                "01098765432",
                TestPasswordHelper.Hash(),
                UserRole.Charity,
                UserType.Company,
                organizationName: "Charity Org",
                registrationNumber: "REG-555");

            var volunteer1Account = Account.Create(
                "Volunteer 1",
                vol1Email,
                "01011111111",
                TestPasswordHelper.Hash(),
                UserRole.Volunteer,
                vehicleType: "Bicycle",
                vehicleDetails: "Hybrid bike");

            var volunteer2Account = Account.Create(
                "Volunteer 2",
                vol2Email,
                "01022222222",
                TestPasswordHelper.Hash(),
                UserRole.Volunteer,
                vehicleType: "Car",
                vehicleDetails: "Sedan");

            var donation = Donation.Create(
                donorAccount.Id,
                "Vegetables",
                "30 kg",
                DateTime.UtcNow.AddDays(1),
                "Farm Loc",
                1,
                "Vegetable donation",
                "Fresh vegetables for distribution",
                1);

            donation.Claim(charityAccount.Id);
            donation.AssignVolunteer(volunteer1Account.Id);

            var mission = DeliveryMission.Create(
                donation.Id,
                volunteer1Account.Id,
                donation.PickupLocation,
                "Charity Kitchen");
            mission.AttachTimeoutJob("hangfire-job-999");

            db.Accounts.AddRange(donorAccount, charityAccount, volunteer1Account, volunteer2Account);
            db.Donations.Add(donation);
            db.DeliveryMissions.Add(mission);
            await db.SaveChangesAsync();

            donationId = donation.Id;
            missionId = mission.Id;
            vol1AccountId = volunteer1Account.Id;
            vol2AccountId = volunteer2Account.Id;
        });

        using (var scope = Factory.Services.CreateScope())
        {
            var timeoutJob = scope.ServiceProvider.GetRequiredService<VolunteerTimeoutJob>();
            await timeoutJob.ExecuteAsync(missionId, vol1AccountId);
        }

        await ExecuteDbContextAsync(async db =>
        {
            var donation = await db.Donations.FindAsync(donationId);
            donation.Should().NotBeNull();
            donation!.Status.Should().Be(DonationStatus.SearchingForVolunteer);
            donation.VolunteerId.Should().BeNull();

            var volunteer1 = await db.Accounts.FindAsync(vol1AccountId);
            volunteer1.Should().NotBeNull();
            volunteer1!.IsAvailable.Should().BeTrue();
        });

        var vol2Client = CreateAuthenticatedClient(vol2AccountId, vol2Email, UserRole.Volunteer);
        var reassignResponse = await vol2Client.PostAsync($"/api/donation/{donationId}/assign", null);
        reassignResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        await ExecuteDbContextAsync(async db =>
        {
            var updatedDonation = await db.Donations.FindAsync(donationId);
            updatedDonation.Should().NotBeNull();
            updatedDonation!.Status.Should().Be(DonationStatus.VolunteerAssigned);
            updatedDonation.VolunteerId.Should().Be(vol2AccountId);
        });
    }
}
