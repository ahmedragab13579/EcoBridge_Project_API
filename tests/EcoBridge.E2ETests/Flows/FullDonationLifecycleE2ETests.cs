using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using EcoBridge.Application.DTO;
using EcoBridge.Application.Result;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Models.Accounts;
using EcoBridge.E2ETests.Common;
using EcoBridge.E2ETests.Fixtures;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EcoBridge.E2ETests.Flows;

public class FullDonationLifecycleE2ETests(CustomE2EWebApplicationFactory factory) : BaseE2ETest(factory)
{
    [Fact]
    public async Task E2E_Full_Donation_Lifecycle_From_Posting_To_Delivery_Should_Succeed()
    {
        var donorEmail = $"donor_e2e_{Guid.NewGuid()}@ecobridge.org";
        var charityEmail = $"charity_e2e_{Guid.NewGuid()}@ecobridge.org";
        var volunteerEmail = $"vol_e2e_{Guid.NewGuid()}@ecobridge.org";

        Guid donorAccountId = Guid.Empty;
        Guid charityAccountId = Guid.Empty;
        Guid volunteerAccountId = Guid.Empty;

        await ExecuteDbContextAsync(async db =>
        {
            var donorAccount = Account.Create(
                "E2E Restaurant",
                donorEmail,
                "01011111111",
                TestPasswordHelper.Hash(),
                UserRole.Donor);

            var charityAccount = Account.Create(
                "E2E Egyptian Food Bank",
                charityEmail,
                "01022222222",
                TestPasswordHelper.Hash(),
                UserRole.Charity,
                UserType.Company,
                organizationName: "E2E Egyptian Food Bank",
                registrationNumber: "REG-123");

            var volunteerAccount = Account.Create(
                "E2E Hero Volunteer",
                volunteerEmail,
                "01033333333",
                TestPasswordHelper.Hash(),
                UserRole.Volunteer,
                vehicleType: "Bicycle",
                vehicleDetails: "Mountain bike");

            db.Accounts.AddRange(donorAccount, charityAccount, volunteerAccount);
            await db.SaveChangesAsync();

            donorAccountId = donorAccount.Id;
            charityAccountId = charityAccount.Id;
            volunteerAccountId = volunteerAccount.Id;
        });

        var donorClient = CreateAuthenticatedClient(donorAccountId, donorEmail, UserRole.Donor);
        var charityClient = CreateAuthenticatedClient(charityAccountId, charityEmail, UserRole.Charity);
        var volunteerClient = CreateAuthenticatedClient(volunteerAccountId, volunteerEmail, UserRole.Volunteer);

        using var postForm = new MultipartFormDataContent();
        postForm.Add(new StringContent("1"), "CategoryId");
        postForm.Add(new StringContent("Surplus Event Lunch"), "Title");
        postForm.Add(new StringContent("Cooked catering lunches for the local shelter"), "Description");
        postForm.Add(new StringContent("Cooked Catering Meals"), "FoodType");
        postForm.Add(new StringContent("40 packed boxes"), "Quantity");
        postForm.Add(new StringContent(DateTime.UtcNow.AddDays(2).ToString("O")), "ExpiryDate");
        postForm.Add(new StringContent("10 El Tahrir Square, Cairo"), "PickupLocation");
        postForm.Add(new StringContent("1"), "CityId");

        var createResponse = await donorClient.PostAsync("/api/donation", postForm);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResult<DonationResponseDTO>>();
        createResult.Should().NotBeNull();
        createResult!.IsSuccess.Should().BeTrue();
        var donationId = createResult.Data!.Id;

        var claimResponse = await charityClient.PostAsync($"/api/donation/{donationId}/accept", null);
        claimResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var assignResponse = await volunteerClient.PostAsync($"/api/donation/{donationId}/assign", null);
        assignResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        Guid missionId = Guid.Empty;
        await ExecuteDbContextAsync(async db =>
        {
            var mission = await db.DeliveryMissions
                .FirstOrDefaultAsync(m => m.DonationId == donationId);

            mission.Should().NotBeNull();
            missionId = mission!.Id;
        });

        var pickupResponse = await volunteerClient.PostAsync($"/api/missions/{missionId}/pickup", null);
        pickupResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var otpResponse = await charityClient.GetAsync($"/api/missions/{missionId}/otp");
        otpResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var otpResult = await otpResponse.Content.ReadFromJsonAsync<ApiResult<string>>();
        otpResult.Should().NotBeNull();
        otpResult!.IsSuccess.Should().BeTrue();
        otpResult.Data.Should().NotBeNullOrWhiteSpace();

        var completeDto = new CompleteMissionRequestDTO { OtpCode = otpResult.Data! };
        var completeResponse = await volunteerClient.PostAsJsonAsync($"/api/missions/{missionId}/complete", completeDto);
        completeResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        await ExecuteDbContextAsync(async db =>
        {
            var finalDonation = await db.Donations.FindAsync(donationId);
            finalDonation.Should().NotBeNull();
            finalDonation!.Status.Should().Be(DonationStatus.Completed);

            var finalMission = await db.DeliveryMissions.FindAsync(missionId);
            finalMission.Should().NotBeNull();
            finalMission!.Status.Should().Be(MissionStatus.Delivered);
            finalMission.DeliveryTime.Should().NotBeNull();
        });
    }
}
