using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using EcoBridge.Application.DTO.Statics.Dashboard;
using EcoBridge.Application.DTO.Statics.Gamification;
using EcoBridge.Application.DTO.Statics.Lookups;
using EcoBridge.Application.DTO.Users.Auth;
using EcoBridge.Application.Result;
using EcoBridge.Application.Results;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Models.Accounts;
using EcoBridge.IntegrationTests.Common;
using EcoBridge.IntegrationTests.Fixtures;
using EcoBridge.IntegrationTests.Helpers;
using FluentAssertions;
using Xunit;

namespace EcoBridge.IntegrationTests.Controllers;

public class ReadOnlyControllerIntegrationTests(CustomWebApplicationFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task DonorDashboard_Should_Return_Current_Account_Metrics()
    {
        var email = $"dashboard_{Guid.NewGuid()}@test.com";
        var accountId = Guid.Empty;

        await ExecuteDbContextAsync(async db =>
        {
            var account = Account.Create("Dashboard Donor", email, "010", TestPasswordHelper.Hash(), UserRole.Donor);
            db.Accounts.Add(account);
            await db.SaveChangesAsync();
            accountId = account.Id;
        });

        var client = CreateAuthenticatedClient(accountId, email, UserRole.Donor);
        var response = await client.GetAsync("/api/dashboard/donor");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResult<DonorDashboardDTO>>();
        result.Should().NotBeNull();
        result!.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.TrustScore.Should().Be(100);
    }

    [Fact]
    public async Task Dashboard_Should_Reject_A_User_With_The_Wrong_Role()
    {
        var client = CreateAuthenticatedClient(Guid.NewGuid(), "donor@test.com", UserRole.Donor);

        var response = await client.GetAsync("/api/dashboard/volunteer");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task PublicProfile_Should_Return_The_Current_User_Profile()
    {
        var email = $"public_{Guid.NewGuid()}@test.com";
        var accountId = Guid.Empty;

        await ExecuteDbContextAsync(async db =>
        {
            var account = Account.Create("Public Donor", email, "010", TestPasswordHelper.Hash(), UserRole.Donor);
            db.Accounts.Add(account);
            await db.SaveChangesAsync();
            accountId = account.Id;
        });

        var response = await Client.GetAsync($"/api/user/{accountId}/public-profile");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResult<PublicProfileDTO>>();
        result.Should().NotBeNull();
        result!.IsSuccess.Should().BeTrue();
        result.Data!.Id.Should().Be(accountId);
        result.Data.DisplayName.Should().Be("Public Donor");
        result.Data.Role.Should().Be(UserRole.Donor.ToString());
    }

    [Fact]
    public async Task Leaderboard_Should_Be_Anonymous_And_Return_Paged_Data()
    {
        var email = $"leaderboard_{Guid.NewGuid()}@test.com";
        await ExecuteDbContextAsync(async db =>
        {
            db.Accounts.Add(Account.Create("Leaderboard Volunteer", email, "011", TestPasswordHelper.Hash(), UserRole.Volunteer));
            await db.SaveChangesAsync();
        });

        var response = await Client.GetAsync("/api/gamification/leaderboard?role=Volunteer&page=1&pagesize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResult<PagedResult<LeaderboardUserDTO>>>();
        result.Should().NotBeNull();
        result!.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.PageNumber.Should().Be(1);
        result.Data.PageSize.Should().BeGreaterThan(0);
        result.Data.Items.Should().Contain(x => x.FullName == "Leaderboard Volunteer");
    }

    [Fact]
    public async Task LookupCategories_Should_Return_A_Successful_List_Response()
    {
        var response = await Client.GetAsync("/api/lookups/categories");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResult<System.Collections.Generic.List<LookupDTO>>>();
        result.Should().NotBeNull();
        result!.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
    }
}
