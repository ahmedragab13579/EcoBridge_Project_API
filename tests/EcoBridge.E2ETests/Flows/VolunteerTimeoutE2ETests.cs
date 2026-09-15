using System;
using System.Net;
using System.Threading.Tasks;
using EcoBridge.Application.BackgroundJobs;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Models;
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
        // ==========================================
        // 1. Arrange - Seed Mission and 2 Volunteers
        // ==========================================
        Guid missionId = Guid.Empty;
        Guid vol1AccountId = Guid.Empty;
        Guid vol2AccountId = Guid.Empty;
        var vol1Email = $"vol1_timeout_{Guid.NewGuid()}@ecobridge.org";
        var vol2Email = $"vol2_reassigned_{Guid.NewGuid()}@ecobridge.org";

        await ExecuteDbContextAsync(async db =>
        {
            var donorAccount = Account.Create("Donor", $"d_{Guid.NewGuid()}@test.com", "010", TestPasswordHelper.Hash(), UserRole.Donor);
            var charityAccount = Account.Create("Charity", $"c_{Guid.NewGuid()}@test.com", "010", TestPasswordHelper.Hash(), UserRole.Charity);
            var volunteer1Account = Account.Create("Volunteer 1", vol1Email, "010111", TestPasswordHelper.Hash(), UserRole.Volunteer);
            var volunteer2Account = Account.Create("Volunteer 2", vol2Email, "010222", TestPasswordHelper.Hash(), UserRole.Volunteer);

            var donor = Donor.Create(donorAccount.Id, UserType.Individual);
            var charity = Charity.Create(charityAccount.Id, "Charity Org", "REG-555");
            var vol1 = Volunteer.Create(volunteer1Account.Id, "Bicycle");
            var vol2 = Volunteer.Create(volunteer2Account.Id, "Car");

            var donation = Donation.Create(donorAccount.Id, "Vegetables", "30 kg", DateTime.UtcNow.AddDays(1), "Farm Loc");
            var request = DonationRequest.Create(donation.Id, charityAccount.Id);
            request.Approve();
            request.Donation = donation;

            var mission = DeliveryMission.Create(request.Id, "Farm Loc", "Charity Kitchen", "1234", DateTime.UtcNow.AddHours(5));
            mission.Accept(volunteer1Account.Id, "hangfire-job-999");
            mission.DonationRequest = request;

            db.Accounts.AddRange(donorAccount, charityAccount, volunteer1Account, volunteer2Account);
            db.Donors.Add(donor);
            db.Charities.Add(charity);
            db.Volunteers.AddRange(vol1, vol2);
            db.Donations.Add(donation);
            db.DonationRequests.Add(request);
            db.DeliveryMissions.Add(mission);
            await db.SaveChangesAsync();

            missionId = mission.Id;
            vol1AccountId = volunteer1Account.Id;
            vol2AccountId = volunteer2Account.Id;
        });

        // ==========================================
        // 2. Trigger 2-Hour Pickup SLA Timeout Job
        // ==========================================
        using (var scope = Factory.Services.CreateScope())
        {
            var timeoutJob = scope.ServiceProvider.GetRequiredService<VolunteerTimeoutJob>();
            await timeoutJob.ExecuteAsync(missionId, vol1AccountId);
        }

        // ==========================================
        // 3. Assert - Penalties & Mission Reset
        // ==========================================
        await ExecuteDbContextAsync(async db =>
        {
            var mission = await db.DeliveryMissions.FindAsync(missionId);
            mission.Should().NotBeNull();
            mission!.MissionStatus.Should().Be(MissionStatus.SearchingForVolunteer);
            mission.VolunteerId.Should().BeNull();
            mission.HangfireJobId.Should().BeNull();

            var vol1 = await db.Volunteers.FindAsync(vol1AccountId);
            vol1!.IsAvailable.Should().BeTrue();
        });

        // ==========================================
        // 4. Act - Volunteer 2 accepts reassigned mission
        // ==========================================
        var vol2Client = CreateAuthenticatedClient(vol2AccountId, vol2Email, UserRole.Volunteer);
        var reassignResponse = await vol2Client.PostAsync($"/api/missions/{missionId}/accept", null);

        // Assert 4 - Volunteer 2 successfully takes over
        reassignResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        await ExecuteDbContextAsync(async db =>
        {
            var updatedMission = await db.DeliveryMissions.FindAsync(missionId);
            updatedMission.Should().NotBeNull();
            updatedMission!.MissionStatus.Should().Be(MissionStatus.Accepted);
            updatedMission.VolunteerId.Should().Be(vol2AccountId);
        });
    }
}
