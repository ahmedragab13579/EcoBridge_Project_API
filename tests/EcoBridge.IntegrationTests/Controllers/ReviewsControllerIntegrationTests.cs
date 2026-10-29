using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using EcoBridge.Application.DTO.WorkFlow;
using EcoBridge.Application.Result;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Models;
using EcoBridge.Domain.Models.Accounts;
using EcoBridge.Domain.Models.Donation;
using EcoBridge.Domain.Models.Mission;
using EcoBridge.IntegrationTests.Common;
using EcoBridge.IntegrationTests.Fixtures;
using EcoBridge.IntegrationTests.Helpers;
using FluentAssertions;
using Xunit;

namespace EcoBridge.IntegrationTests.Controllers;

public class ReviewsControllerIntegrationTests(CustomWebApplicationFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task Completed_Mission_Participant_Should_Create_A_Review()
    {
        var reviewerEmail = $"reviewer_{Guid.NewGuid()}@test.com";
        var reviewerId = Guid.Empty;
        var revieweeId = Guid.Empty;
        var missionId = Guid.Empty;
        var reasonId = 0;

        await ExecuteDbContextAsync(async db =>
        {
            var donor = Account.Create("Donor", reviewerEmail, "010", TestPasswordHelper.Hash(), UserRole.Donor);
            var charity = Account.Create("Charity", $"charity_{Guid.NewGuid()}@test.com", "011", TestPasswordHelper.Hash(), UserRole.Charity, UserType.Company, organizationName: "Food Bank", registrationNumber: "REG-1");
            var volunteer = Account.Create("Volunteer", $"volunteer_{Guid.NewGuid()}@test.com", "012", TestPasswordHelper.Hash(), UserRole.Volunteer);
            var reason = ReviewReason.Create("PUNCTUALNAME", UserRole.Donor, UserRole.Volunteer, true);
            var donation = Donation.Create(donor.Id, "Meals", "10 boxes", DateTime.UtcNow.AddDays(2), "Pickup", 1, "Meals", "Prepared meals", 1);
            donation.Claim(charity.Id);
            donation.AssignVolunteer(volunteer.Id);
            donation.Complete();
            var mission = DeliveryMission.Create(donation.Id, volunteer.Id, "Pickup", "Dropoff");
            mission.ConfirmPickup();
            mission.RecordDelivery();

            db.Accounts.AddRange(donor, charity, volunteer);
            db.ReviewReasons.Add(reason);
            db.Donations.Add(donation);
            db.DeliveryMissions.Add(mission);
            await db.SaveChangesAsync();

            reviewerId = donor.Id;
            revieweeId = volunteer.Id;
            missionId = mission.Id;
            reasonId = reason.Id;
        });

        var client = CreateAuthenticatedClient(reviewerId, reviewerEmail, UserRole.Donor);
        var response = await client.PostAsJsonAsync("/api/review", new CreateReviewDTO
        {
            MissionId = missionId,
            RevieweeId = revieweeId,
            ReviewReasonId = reasonId,
            Rating = 5,
            Comment = "Very punctual volunteer"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResult<ReviewResponseDTO>>();
        result.Should().NotBeNull();
        result!.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.ReviewerId.Should().Be(reviewerId);
        result.Data.RevieweeId.Should().Be(revieweeId);
        result.Data.MissionId.Should().Be(missionId);
    }

    [Fact]
    public async Task NonParticipant_Should_Not_Create_A_Review_For_A_Mission()
    {
        var outsiderId = Guid.NewGuid();
        var client = CreateAuthenticatedClient(outsiderId, "outsider@test.com", UserRole.Donor);

        var response = await client.PostAsJsonAsync("/api/review", new CreateReviewDTO
        {
            MissionId = Guid.NewGuid(),
            RevieweeId = Guid.NewGuid(),
            ReviewReasonId = 1,
            Rating = 5
        });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
