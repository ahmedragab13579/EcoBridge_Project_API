using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using EcoBridge.Application.DTO;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Models;
using EcoBridge.Domain.Results;
using EcoBridge.E2ETests.Common;
using EcoBridge.E2ETests.Fixtures;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EcoBridge.E2ETests.Flows;

public class FullDonationLifecycleE2ETests(CustomE2EWebApplicationFactory factory) : BaseE2ETest(factory)
{
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    [Fact]
    public async Task E2E_Full_Donation_Lifecycle_From_Posting_To_Delivery_Should_Succeed()
    {
        // ==========================================
        // 1. Setup Actors: Donor, Charity, Volunteer
        // ==========================================
        var donorEmail = $"donor_e2e_{Guid.NewGuid()}@ecobridge.org";
        var charityEmail = $"charity_e2e_{Guid.NewGuid()}@ecobridge.org";
        var volunteerEmail = $"vol_e2e_{Guid.NewGuid()}@ecobridge.org";

        Guid donorAccountId = Guid.Empty;
        Guid charityAccountId = Guid.Empty;
        Guid volunteerAccountId = Guid.Empty;

        await ExecuteDbContextAsync(async db =>
        {
            var donorAccount = Account.Create("E2E Restaurant", donorEmail, "01011111111", TestPasswordHelper.Hash(), UserRole.Donor);
            var donor = Donor.Create(donorAccount.Id, UserType.Company, "E2E Restaurant", "10 El Tahrir Square, Cairo");

            var charityAccount = Account.Create("E2E Egyptian Food Bank", charityEmail, "01022222222", TestPasswordHelper.Hash(), UserRole.Charity);
            var charity = Charity.Create(charityAccount.Id, "E2E Egyptian Food Bank", "EFB-2026-99", "Tagamoa, New Cairo");

            var volunteerAccount = Account.Create("E2E Hero Volunteer", volunteerEmail, "01033333333", TestPasswordHelper.Hash(), UserRole.Volunteer);
            var volunteer = Volunteer.Create(volunteerAccount.Id, "Van", "Ford Transit 2023");

            db.Accounts.AddRange(donorAccount, charityAccount, volunteerAccount);
            db.Donors.Add(donor);
            db.Charities.Add(charity);
            db.Volunteers.Add(volunteer);
            await db.SaveChangesAsync();

            donorAccountId = donorAccount.Id;
            charityAccountId = charityAccount.Id;
            volunteerAccountId = volunteerAccount.Id;
        });

        var donorClient = CreateAuthenticatedClient(donorAccountId, donorEmail, UserRole.Donor);
        var charityClient = CreateAuthenticatedClient(charityAccountId, charityEmail, UserRole.Charity);
        var volunteerClient = CreateAuthenticatedClient(volunteerAccountId, volunteerEmail, UserRole.Volunteer);

        // ==========================================
        // 2. Donor posts a donation
        // ==========================================
        using var postForm = new MultipartFormDataContent();
        postForm.Add(new StringContent("Cooked Catering Meals"), "FoodType");
        postForm.Add(new StringContent("40 packed boxes"), "Quantity");
        postForm.Add(new StringContent(DateTime.UtcNow.AddDays(2).ToString("O")), "ExpiryDate");
        postForm.Add(new StringContent("10 El Tahrir Square, Cairo"), "PickupLocation");
        postForm.Add(new StringContent("Surplus Event Lunch"), "Title");

        var createResponse = await donorClient.PostAsync("/api/donation", postForm);
        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResult<DonationResponseDTO>>(JsonOpts);
        createResult.Should().NotBeNull();
        createResult!.IsSuccess.Should().BeTrue();
        var donationId = createResult.Data!.Id;

        // ==========================================
        // 3. Charity views donations & sends request
        // ==========================================
        var requestResponse = await charityClient.PostAsync($"/api/donation/{donationId}/request", null);
        requestResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var requestResult = await requestResponse.Content.ReadFromJsonAsync<ApiResult<Guid>>(JsonOpts);
        requestResult.Should().NotBeNull();
        requestResult!.IsSuccess.Should().BeTrue();
        var donationRequestId = requestResult.Data;

        // ==========================================
        // 4. Donor approves Charity request
        // ==========================================
        var approveResponse = await donorClient.PostAsync($"/api/DonationRequests/{donationRequestId}/approve", null);
        approveResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var approveResult = await approveResponse.Content.ReadFromJsonAsync<ApiResult<Guid>>(JsonOpts);
        approveResult.Should().NotBeNull();
        approveResult!.IsSuccess.Should().BeTrue();
        var missionId = approveResult.Data;

        // ==========================================
        // 5. Volunteer accepts the mission
        // ==========================================
        var acceptResponse = await volunteerClient.PostAsync($"/api/missions/{missionId}/accept", null);
        acceptResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // ==========================================
        // 6. Donor confirms pickup
        // ==========================================
        var pickupResponse = await donorClient.PostAsync($"/api/missions/{missionId}/confirm-pickup", null);
        pickupResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // ==========================================
        // 7. Charity checks OTP code
        // ==========================================
        var otpResponse = await charityClient.GetAsync($"/api/missions/{missionId}/otp");
        otpResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var otpJson = await otpResponse.Content.ReadFromJsonAsync<JsonElement>();
        var otpCode = otpJson.GetProperty("data").GetProperty("otpCode").GetString();
        otpCode.Should().NotBeNullOrWhiteSpace();

        // ==========================================
        // 8. Volunteer completes delivery using OTP
        // ==========================================
        var completeDto = new CompleteMissionRequestDTO { OtpCode = otpCode! };
        var completeResponse = await volunteerClient.PostAsJsonAsync($"/api/missions/{missionId}/complete", completeDto);
        completeResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // ==========================================
        // 9. Final Assertions on Database State
        // ==========================================
        await ExecuteDbContextAsync(async db =>
        {
            var finalDonation = await db.Donations.FindAsync(donationId);
            finalDonation.Should().NotBeNull();
            finalDonation!.Status.Should().Be(DonationStatus.Delivered);

            var finalMission = await db.DeliveryMissions.FindAsync(missionId);
            finalMission.Should().NotBeNull();
            finalMission!.MissionStatus.Should().Be(MissionStatus.Delivered);
            finalMission.DeliveryTime.Should().NotBeNull();

            var finalRequest = await db.DonationRequests.FindAsync(donationRequestId);
            finalRequest.Should().NotBeNull();
            finalRequest!.RequestStatus.Should().Be(RequestStatus.ApprovedByDonor);
        });
    }
}
