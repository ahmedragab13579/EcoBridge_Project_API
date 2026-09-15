using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using EcoBridge.Application.DTO;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Models;
using EcoBridge.Domain.Results;
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
        // Arrange - Seed donor, charity, volunteer, donation, request, mission
        Guid donorId = Guid.Empty;
        Guid volunteerId = Guid.Empty;
        Guid missionId = Guid.Empty;
        var donorEmail = $"donor_{Guid.NewGuid()}@ecobridge.org";
        var volunteerEmail = $"vol_{Guid.NewGuid()}@ecobridge.org";
        const string otpCode = "9182";

        await ExecuteDbContextAsync(async db =>
        {
            var donorAccount = Account.Create("Donor", donorEmail, "010", TestPasswordHelper.Hash(), UserRole.Donor);
            var charityAccount = Account.Create("Charity", $"c_{Guid.NewGuid()}@test.com", "010", TestPasswordHelper.Hash(), UserRole.Charity);
            var volunteerAccount = Account.Create("Volunteer", volunteerEmail, "010", TestPasswordHelper.Hash(), UserRole.Volunteer);

            var donor = Donor.Create(donorAccount.Id, UserType.Individual);
            var charity = Charity.Create(charityAccount.Id, "Charity", "REG-777");
            var volunteer = Volunteer.Create(volunteerAccount.Id, "Scooter");

            var donation = Donation.Create(donorAccount.Id, "Hot Meals", "15 boxes", DateTime.UtcNow.AddDays(1), "Pickup Address");
            var request = DonationRequest.Create(donation.Id, charityAccount.Id);
            request.Approve();
            request.Donation = donation;

            var mission = DeliveryMission.Create(request.Id, "Pickup Address", "Charity Dropoff", otpCode, DateTime.UtcNow.AddHours(12));
            mission.Accept(volunteerAccount.Id);
            mission.DonationRequest = request;

            db.Accounts.AddRange(donorAccount, charityAccount, volunteerAccount);
            db.Donors.Add(donor);
            db.Charities.Add(charity);
            db.Volunteers.Add(volunteer);
            db.Donations.Add(donation);
            db.DonationRequests.Add(request);
            db.DeliveryMissions.Add(mission);
            await db.SaveChangesAsync();

            donorId = donorAccount.Id;
            volunteerId = volunteerAccount.Id;
            missionId = mission.Id;
        });

        var donorClient = CreateAuthenticatedClient(donorId, donorEmail, UserRole.Donor);
        var volunteerClient = CreateAuthenticatedClient(volunteerId, volunteerEmail, UserRole.Volunteer);

        // Act 1 - Donor confirms pickup
        var pickupResponse = await donorClient.PostAsync($"/api/missions/{missionId}/confirm-pickup", null);

        // Assert 1
        pickupResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Act 2 - Volunteer completes delivery with correct OTP
        var completeDto = new CompleteMissionRequestDTO { OtpCode = otpCode };
        var completeResponse = await volunteerClient.PostAsJsonAsync($"/api/missions/{missionId}/complete", completeDto);

        // Assert 2
        completeResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify status updated in DB
        await ExecuteDbContextAsync(async db =>
        {
            var updatedMission = await db.DeliveryMissions.FindAsync(missionId);
            updatedMission.Should().NotBeNull();
            updatedMission!.MissionStatus.Should().Be(MissionStatus.Delivered);
            updatedMission.DeliveryTime.Should().NotBeNull();
        });
    }
}
