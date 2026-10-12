using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using EcoBridge.Application.DTO;
using EcoBridge.Application.Result;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Models.Accounts;
using EcoBridge.Domain.Models.Donation;
using EcoBridge.IntegrationTests.Common;
using EcoBridge.IntegrationTests.Fixtures;
using EcoBridge.IntegrationTests.Helpers;
using FluentAssertions;
using Xunit;

namespace EcoBridge.IntegrationTests.Controllers;

public class DonationControllerIntegrationTests(CustomWebApplicationFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task CreateDonation_Without_Auth_Should_Return_401_Unauthorized()
    {
        using var form = CreateDonationForm();
        var response = await Client.PostAsync("/api/donation", form);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateDonation_As_Volunteer_Should_Return_403_Forbidden()
    {
        var client = CreateAuthenticatedClient(Guid.NewGuid(), "vol@test.com", UserRole.Volunteer);
        using var form = CreateDonationForm();
        var response = await client.PostAsync("/api/donation", form);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateDonation_As_Donor_Should_Return_Created_Donation()
    {
        var email = $"donor_{Guid.NewGuid()}@test.com";
        var donorId = Guid.Empty;
        await ExecuteDbContextAsync(async db =>
        {
            var account = Account.Create("Donor", email, "010", TestPasswordHelper.Hash(), UserRole.Donor);
            db.Accounts.Add(account);
            await db.SaveChangesAsync();
            donorId = account.Id;
        });

        var client = CreateAuthenticatedClient(donorId, email, UserRole.Donor);
        using var form = CreateDonationForm();
        var response = await client.PostAsync("/api/donation", form);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<ApiResult<DonationResponseDTO>>();
        result.Should().NotBeNull();
        result!.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.FoodType.Should().Be("Fresh Bread");
        result.Data.QuantityDes.Should().Be("10 boxes");
    }

    [Fact]
    public async Task AcceptDonation_As_Charity_Should_Claim_Available_Donation()
    {
        var charityEmail = $"charity_{Guid.NewGuid()}@test.com";
        var charityId = Guid.Empty;
        var donationId = Guid.Empty;

        await ExecuteDbContextAsync(async db =>
        {
            var donor = Account.Create("Donor", $"donor_{Guid.NewGuid()}@test.com", "010", TestPasswordHelper.Hash(), UserRole.Donor);
            var charity = Account.Create("Food Bank", charityEmail, "011", TestPasswordHelper.Hash(), UserRole.Charity, UserType.Company, organizationName: "Food Bank", registrationNumber: "REG-1");
            var donation = Donation.Create(donor.Id, "Canned Soups", "50 cans", DateTime.UtcNow.AddDays(30), "Distribution Hub", 1, "Canned soup", "Shelf stable food", 1);
            db.Accounts.AddRange(donor, charity);
            db.Donations.Add(donation);
            await db.SaveChangesAsync();
            charityId = charity.Id;
            donationId = donation.Id;
        });

        var client = CreateAuthenticatedClient(charityId, charityEmail, UserRole.Charity);
        var response = await client.PostAsync($"/api/donation/{donationId}/accept", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResult<bool>>();
        result.Should().NotBeNull();
        result!.IsSuccess.Should().BeTrue();
        result.Data.Should().BeTrue();
    }

    private static MultipartFormDataContent CreateDonationForm()
    {
        var form = new MultipartFormDataContent();
        form.Add(new StringContent("Fresh Bread"), "FoodType");
        form.Add(new StringContent("10 boxes"), "Quantity");
        form.Add(new StringContent(DateTime.UtcNow.AddDays(2).ToString("O")), "ExpiryDate");
        form.Add(new StringContent("123 Street"), "PickupLocation");
        form.Add(new StringContent("1"), "CityId");
        form.Add(new StringContent("1"), "CategoryId");
        form.Add(new StringContent("Bread donation"), "Title");
        form.Add(new StringContent("Fresh bread"), "Description");
        return form;
    }
}
