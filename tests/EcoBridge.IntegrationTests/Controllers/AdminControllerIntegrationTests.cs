using System;
using System.Net;
using System.Threading.Tasks;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Models.Accounts;
using EcoBridge.IntegrationTests.Common;
using EcoBridge.IntegrationTests.Fixtures;
using EcoBridge.IntegrationTests.Helpers;
using FluentAssertions;
using Xunit;

namespace EcoBridge.IntegrationTests.Controllers;

public class AdminControllerIntegrationTests(CustomWebApplicationFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task GetStats_As_Donor_Should_Return_403_Forbidden()
    {
        // Arrange
        var donorClient = CreateAuthenticatedClient(Guid.NewGuid(), "donor@test.com", UserRole.Donor);

        // Act
        var response = await donorClient.GetAsync("/api/admin/stats");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetStats_As_Admin_Should_Return_200_OK()
    {
        // Arrange
        var adminClient = CreateAuthenticatedClient(Guid.NewGuid(), "admin@ecobridge.org", UserRole.Admin);

        // Act
        var response = await adminClient.GetAsync("/api/admin/stats");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ToggleUserStatus_As_Admin_Should_Succeed()
    {
        // Arrange - Seed user to toggle
        Guid targetUserId = Guid.Empty;
        await ExecuteDbContextAsync(async db =>
        {
            var account = Account.Create("Target User", $"target_{Guid.NewGuid()}@test.com", "010", TestPasswordHelper.Hash(), UserRole.Volunteer);
            db.Accounts.Add(account);
            await db.SaveChangesAsync();
            targetUserId = account.Id;
        });

        var adminClient = CreateAuthenticatedClient(Guid.NewGuid(), "admin@ecobridge.org", UserRole.Admin);

        // Act
        var response = await adminClient.PatchAsync($"/api/admin/users/{targetUserId}/toggle-status", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        await ExecuteDbContextAsync(async db =>
        {
            var updated = await db.Accounts.FindAsync(targetUserId);
            updated.Should().NotBeNull();
            updated!.IsActive.Should().BeFalse(); // Was active, now toggled to inactive
        });
    }
}
