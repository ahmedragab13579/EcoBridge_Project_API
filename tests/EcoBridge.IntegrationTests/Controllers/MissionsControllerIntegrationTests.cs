using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using EcoBridge.Application.DTO;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Models.Accounts;
using EcoBridge.Domain.Models.Donation;
using EcoBridge.Domain.Models.Mission;
using EcoBridge.IntegrationTests.Common;
using EcoBridge.IntegrationTests.Fixtures;
using EcoBridge.IntegrationTests.Helpers;
using FluentAssertions;
using Xunit;

namespace EcoBridge.IntegrationTests.Controllers;

public class MissionsControllerIntegrationTests(CustomWebApplicationFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task ConfirmPickup_And_CompleteMission_Flow_Should_Succeed()
    {
        // Arrange - Seed accounts, donation, and assigned mission
        DeliveryMission mission = null;
        Guid volunteerId = Guid.Empty;
        Guid charityId = Guid.Empty;
        Guid missionId = Guid.Empty;
        var volunteerEmail = $"vol_{Guid.NewGuid()}@ecobridge.org";
        var charityEmail = $"charity_{Guid.NewGuid()}@ecobridge.org";

        await ExecuteDbContextAsync(async db =>
        {
            var volunteerAccount = Account.Create("Volunteer", volunteerEmail, "010", TestPasswordHelper.Hash(), UserRole.Volunteer);
            var charityAccount = Account.Create("Charity", charityEmail, "011", TestPasswordHelper.Hash(), UserRole.Charity, UserType.Company, organizationName: "Food Bank", registrationNumber: "REG-1");
            var donorAccount = Account.Create("Donor", $"donor_{Guid.NewGuid()}@test.com", "010", TestPasswordHelper.Hash(), UserRole.Donor);
            var donation = Donation.Create(donorAccount.Id, "Hot Meals", "15 boxes", DateTime.UtcNow.AddDays(1), "Pickup Address", 1, "Hot meals", "Prepared meals", 1);
            donation.Claim(charityAccount.Id);
            donation.AssignVolunteer(volunteerAccount.Id);
            mission = DeliveryMission.Create(donation.Id, volunteerAccount.Id, "Pickup Address", "Charity Dropoff");
            db.Accounts.AddRange(donorAccount, volunteerAccount, charityAccount);
            db.Donations.Add(donation);
            db.DeliveryMissions.Add(mission);
            await db.SaveChangesAsync();

            volunteerId = volunteerAccount.Id;
            charityId = charityAccount.Id;
            missionId = mission.Id;
        });

        var volunteerClient = CreateAuthenticatedClient(volunteerId, volunteerEmail, UserRole.Volunteer);
        var charityClient = CreateAuthenticatedClient(charityId, charityEmail, UserRole.Charity);

        // Act 1 - Volunteer confirms pickup
        var pickupResponse = await volunteerClient.PostAsync($"/api/missions/{missionId}/pickup", null);
        // Assert 1
        pickupResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var otpResponse = await charityClient.GetAsync($"/api/missions/{missionId}/otp");
        otpResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var otpResult = await otpResponse.Content.ReadFromJsonAsync<EcoBridge.Application.Result.ApiResult<string>>();
        otpResult.Should().NotBeNull();
        otpResult!.Data.Should().NotBeNullOrWhiteSpace();

        // Act 2 - Complete delivery using the mission OTP issued by the charity flow.
        var completeResponse = await volunteerClient.PostAsJsonAsync($"/api/missions/{missionId}/complete", new CompleteMissionRequestDTO { OtpCode = otpResult.Data! });

        // Assert 2
        completeResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify status updated in DB
        await ExecuteDbContextAsync(async db =>
        {
            var updatedMission = await db.DeliveryMissions.FindAsync(missionId);
            updatedMission.Should().NotBeNull();
            updatedMission!.Status.Should().Be(MissionStatus.Delivered);
            updatedMission.DeliveryTime.Should().NotBeNull();
        });
    }
}
