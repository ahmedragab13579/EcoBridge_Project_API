using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using EcoBridge.Application.DTO;
using EcoBridge.Application.DTO.Users;
using EcoBridge.Application.DTO.Users.Auth;
using EcoBridge.Application.DTO.Users.Auth.Users;
using EcoBridge.Application.Result;
using EcoBridge.Domain.Enums;
using EcoBridge.E2ETests.Common;
using EcoBridge.E2ETests.Fixtures;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EcoBridge.E2ETests.Flows;

public class AdminGovernanceE2ETests(CustomE2EWebApplicationFactory factory) : BaseE2ETest(factory)
{
    [Fact]
    public async Task Admin_Governance_Deactivation_And_Reactivation_Lifecycle_Should_Succeed()
    {
        // ==========================================
        // 1. Charity Registers
        // ==========================================
        var charityEmail = $"charity_gov_{Guid.NewGuid()}@ecobridge.org";
        var password = "StrongPassword123!";

        var registerDto = new RegisterRequestDTO
        {
            FullName = "New Charity Branch",
            Email = charityEmail,
            Password = password,
            Phone = "01098765432",
            Role = UserRole.Charity.ToString(),
            OrganizationName = "Etihad",
            RegistrationNumber = "5",
            Type = UserType.Company.ToString(),
        };

        var registerResponse = await AnonymousClient.PostAsJsonAsync("/api/auth/register", registerDto);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        Guid charityUserId = Guid.Empty;
        await ExecuteDbContextAsync(async db =>
        {
            var charityAccount = await db.Accounts.FirstOrDefaultAsync(a => a.Email == charityEmail);
            charityAccount.Should().NotBeNull();
            charityUserId = charityAccount!.Id;
        });

        // ==========================================
        // 2. Admin Inspects Platform Stats & Users
        // ==========================================
        var adminId = Guid.NewGuid();
        var adminClient = CreateAuthenticatedClient(adminId, "admin@ecobridge.org", UserRole.Admin);

        var statsResponse = await adminClient.GetAsync("/api/admin/stats");
        statsResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var usersResponse = await adminClient.GetAsync($"/api/admin/users?role={UserRole.Charity}");
        usersResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // ==========================================
        // 3. Admin Deactivates Charity
        // ==========================================
        var toggleDeactivateResponse = await adminClient.PatchAsync($"/api/admin/users/{charityUserId}/toggle-status", null);
        toggleDeactivateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // ==========================================
        // 4. Charity Attempts Login (Should Fail with 403 Forbidden)
        // ==========================================
        var loginDto = new LoginRequestDTO
        {
            Email = charityEmail,
            Password = password
        };

        var failedLoginResponse = await AnonymousClient.PostAsJsonAsync("/api/auth/login", loginDto);
        failedLoginResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        // ==========================================
        // 5. Admin Reactivates Charity
        // ==========================================
        var toggleReactivateResponse = await adminClient.PatchAsync($"/api/admin/users/{charityUserId}/toggle-status", null);
        toggleReactivateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // ==========================================
        // 6. Charity Logs In Successfully
        // ==========================================
        var successfulLoginResponse = await AnonymousClient.PostAsJsonAsync("/api/auth/login", loginDto);
        successfulLoginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var loginResult = await successfulLoginResponse.Content.ReadFromJsonAsync<ApiResult<AuthResponseDTO>>();
        loginResult.Should().NotBeNull();
        loginResult!.IsSuccess.Should().BeTrue();
        loginResult.Data!.AccessToken.Should().NotBeNullOrWhiteSpace();
    }
}
