using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using EcoBridge.Application.DTO;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Models.Accounts;
using EcoBridge.Domain.Models.Donation;
using EcoBridge.Domain.Results;
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
        // Arrange
        using var form = new MultipartFormDataContent();
        form.Add(new StringContent("Fresh Bread"), "FoodType");
        form.Add(new StringContent("10 boxes"), "Quantity");
        form.Add(new StringContent(DateTime.UtcNow.AddDays(2).ToString("O")), "ExpiryDate");
        form.Add(new StringContent("123 Street"), "PickupLocation");

        // Act
        var response = await Client.PostAsync("/api/donation", form);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateDonation_As_Volunteer_Should_Return_403_Forbidden()
    {
        // Arrange
        var volunteerId = Guid.NewGuid();
        var client = CreateAuthenticatedClient(volunteerId, "vol@test.com", UserRole.Volunteer);

        using var form = new MultipartFormDataContent();
        form.Add(new StringContent("Fresh Bread"), "FoodType");
        form.Add(new StringContent("10 boxes"), "Quantity");
        form.Add(new StringContent(DateTime.UtcNow.AddDays(2).ToString("O")), "ExpiryDate");
        form.Add(new StringContent("123 Street"), "PickupLocation");

        // Act
        var response = await client.PostAsync("/api/donation", form);

        // Assert - Only Donors are authorized to POST donations
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateDonation_As_Donor_Should_Return_200_OK()
    {
        // Arrange - Seed donor account
        Guid donorId = Guid.Empty;
        var email = $"donor_{Guid.NewGuid()}@test.com";

        await ExecuteDbContextAsync(async db =>
        {
            var account = Account.Create("Donor Org", email, "010", TestPasswordHelper.Hash(), UserRole.Donor);
            var donor = Donor.Create(account.Id, UserType.Individual);
            donor.Account = account;

            db.Accounts.Add(account);
            db.Donors.Add(donor);
            await db.SaveChangesAsync();

            donorId = account.Id;
        });

        var donorClient = CreateAuthenticatedClient(donorId, email, UserRole.Donor);

        using var form = new MultipartFormDataContent();
        form.Add(new StringContent("Healthy Sandwiches"), "FoodType");
        form.Add(new StringContent("30 meals"), "Quantity");
        form.Add(new StringContent(DateTime.UtcNow.AddDays(2).ToString("O")), "ExpiryDate");
        form.Add(new StringContent("Downtown Restaurant"), "PickupLocation");

        // Act
        var response = await donorClient.PostAsync("/api/donation", form);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResult<DonationResponseDTO>>();
        result.Should().NotBeNull();
        result!.IsSuccess.Should().BeTrue();
        result.Data!.FoodType.Should().Be("Healthy Sandwiches");
        result.Data.Quantity.Should().Be("30 meals");
    }

    [Fact]
    public async Task RequestDonation_As_Charity_Should_Return_200_OK()
    {
        // Arrange - Seed donor, charity, and donation
        Guid charityId = Guid.Empty;
        Guid donationId = Guid.Empty;
        var charityEmail = $"charity_{Guid.NewGuid()}@ecobridge.org";

        await ExecuteDbContextAsync(async db =>
        {
            var donorAccount = Account.Create("Donor", $"donor_{Guid.NewGuid()}@test.com", "010", TestPasswordHelper.Hash(), UserRole.Donor);
            var donor = Donor.Create(donorAccount.Id, UserType.Individual);

            var charityAccount = Account.Create("Food Bank", charityEmail, "010", TestPasswordHelper.Hash(), UserRole.Charity);
            var charity = Charity.Create(charityAccount.Id, "Food Bank", "REG-888");

            var donation = Donation.Create(donorAccount.Id, "Canned Soups", "50 cans", DateTime.UtcNow.AddDays(30), "Distribution Hub");

            db.Accounts.AddRange(donorAccount, charityAccount);
            db.Donors.Add(donor);
            db.Charities.Add(charity);
            db.Donations.Add(donation);
            await db.SaveChangesAsync();

            charityId = charityAccount.Id;
            donationId = donation.Id;
        });

        var charityClient = CreateAuthenticatedClient(charityId, charityEmail, UserRole.Charity);

        // Act
        var response = await charityClient.PostAsync($"/api/donation/{donationId}/request", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResult<Guid>>();
        result.Should().NotBeNull();
        result!.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeEmpty(); // DonationRequestId
    }
}
