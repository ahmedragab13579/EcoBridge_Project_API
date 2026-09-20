using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using EcoBridge.Application.DTO;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Models.Accounts;
using EcoBridge.Domain.Results;
using EcoBridge.IntegrationTests.Common;
using EcoBridge.IntegrationTests.Fixtures;
using EcoBridge.IntegrationTests.Helpers;
using FluentAssertions;
using Xunit;

namespace EcoBridge.IntegrationTests.Controllers;

public class AuthControllerIntegrationTests(CustomWebApplicationFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task Register_And_Login_Flow_Should_Succeed_Via_Http()
    {
        // Arrange
        var email = $"integration_user_{Guid.NewGuid()}@ecobridge.org";
        var registerDto = new RegisterRequestDTO
        {
            FullName = "Integration Donor",
            Email = email,
            Password = "Password123!",
            Phone = "01099998888",
            Role = UserRole.Donor.ToString()
        };

        // Act 1 - Register
        var registerResponse = await Client.PostAsJsonAsync("/api/auth/register", registerDto);

        // Assert 1
        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var registerResult = await registerResponse.Content.ReadFromJsonAsync<ApiResult<AuthResponseDTO>>();
        registerResult.Should().NotBeNull();
        registerResult!.IsSuccess.Should().BeTrue();
        registerResult.Data.Should().NotBeNull();
        registerResult.Data!.AccessToken.Should().NotBeNullOrWhiteSpace();

        // Act 2 - Login with registered credentials
        var loginDto = new LoginRequestDTO
        {
            Email = email,
            Password = "Password123!"
        };
        var loginResponse = await Client.PostAsJsonAsync("/api/auth/login", loginDto);

        // Assert 2
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<ApiResult<AuthResponseDTO>>();
        loginResult.Should().NotBeNull();
        loginResult!.IsSuccess.Should().BeTrue();
        loginResult.Data!.AccessToken.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task GetProfile_Without_Token_Should_Return_401_Unauthorized()
    {
        // Act
        var response = await Client.GetAsync("/api/auth/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetProfile_With_Valid_Token_Should_Return_200_OK()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var email = $"profile_test_{Guid.NewGuid()}@ecobridge.org";

        await ExecuteDbContextAsync(async db =>
        {
            var account = Account.Create("Test Profile User", email, "01000", TestPasswordHelper.Hash(), UserRole.Donor);
            db.Accounts.Add(account);
            await db.SaveChangesAsync();
            userId = account.Id;
        });

        var authenticatedClient = CreateAuthenticatedClient(userId, email, UserRole.Donor);

        // Act
        var response = await authenticatedClient.GetAsync("/api/auth/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResult<UserProfileDTO>>();
        result.Should().NotBeNull();
        result!.IsSuccess.Should().BeTrue();
        result.Data!.FullName.Should().Be("Test Profile User");
    }
}
