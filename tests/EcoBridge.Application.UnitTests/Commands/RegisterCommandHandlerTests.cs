using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EcoBridge.Application.Contracts;
using EcoBridge.Application.DTO;
using EcoBridge.Application.Features.Auth;
using EcoBridge.Application.Features.Auth.Commands;
using EcoBridge.Application.UnitTests.Helpers;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace EcoBridge.Application.UnitTests.Commands;

public class RegisterCommandHandlerTests
{
    private readonly Mock<ITokenService> _tokenServiceMock = new();
    private readonly Mock<IRefreshTokenService> _refreshTokenServiceMock = new();
    private readonly IOptions<JWTSettings> _jwtOptions = Options.Create(new JWTSettings { DurationInMinutes = 60 });

    [Fact]
    public async Task Handle_Should_Return_Validation_Error_When_Attempting_To_Register_As_Admin()
    {
        // Arrange
        using var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var handler = new RegisterCommandHandler(
            dbContext,
            _tokenServiceMock.Object,
            _refreshTokenServiceMock.Object,
            _jwtOptions);

        var command = new RegisterCommand(new RegisterRequestDTO
        {
            FullName = "Admin User",
            Email = "admin@test.com",
            Password = "Password123!",
            Phone = "01000000000",
            Role = UserRole.Admin.ToString()
        });

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.Validation);
        result.Message.Should().Be(AppMessageCodes.VALIDATION_ERROR);
    }

    [Fact]
    public async Task Handle_Should_Return_Conflict_When_Email_Already_Exists()
    {
        // Arrange
        using var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var existingAccount = Account.Create("Existing", "user@test.com", "010", TestPasswordHelper.Hash(), UserRole.Donor);
        dbContext.Accounts.Add(existingAccount);
        await dbContext.SaveChangesAsync();

        var handler = new RegisterCommandHandler(
            dbContext,
            _tokenServiceMock.Object,
            _refreshTokenServiceMock.Object,
            _jwtOptions);

        var command = new RegisterCommand(new RegisterRequestDTO
        {
            FullName = "Duplicate User",
            Email = "user@test.com",
            Password = "Password123!",
            Phone = "01111111111",
            Role = UserRole.Donor.ToString()
        });

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.Conflict);
        result.Message.Should().Be(AppMessageCodes.EMAIL_ALREADY_EXISTS);
    }

    [Fact]
    public async Task Handle_Should_Create_Account_And_Return_Auth_Tokens_When_Valid()
    {
        // Arrange
        using var dbContext = TestDbContextFactory.CreateInMemoryDbContext();

        _tokenServiceMock
            .Setup(t => t.GenerateAccessToken(It.IsAny<Account>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("sample.access.token");

        _refreshTokenServiceMock
            .Setup(r => r.GenerateAndSaveRefreshTokenAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("sample-refresh-token");

        var handler = new RegisterCommandHandler(
            dbContext,
            _tokenServiceMock.Object,
            _refreshTokenServiceMock.Object,
            _jwtOptions);

        var email = $"donor_{Guid.NewGuid()}@ecobridge.org";
        var command = new RegisterCommand(new RegisterRequestDTO
        {
            FullName = "New Donor",
            Email = email,
            Password = "SecurePassword123!",
            Phone = "01012345678",
            Role = UserRole.Donor.ToString(),
            CityId = 1
        });

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Be(AppMessageCodes.REGISTER_SUCCESS);
        result.Value.AccessToken.Should().Be("sample.access.token");
        result.Value.RefreshToken.Should().Be("sample-refresh-token");
        result.Value.AccessTokenExpiresAtUtc.Should().BeAfter(DateTime.UtcNow);

        // Verify entity persisted
        var persisted = await dbContext.Accounts.FirstOrDefaultAsync(a => a.Email == email);
        persisted.Should().NotBeNull();
        persisted!.FullName.Should().Be("New Donor");
        persisted.RoleId.Should().Be(UserRole.Donor);
    }
}
