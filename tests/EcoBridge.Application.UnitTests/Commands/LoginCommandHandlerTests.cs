using System;
using System.Threading;
using System.Threading.Tasks;
using EcoBridge.Application.Contracts;
using EcoBridge.Application.DTO;
using EcoBridge.Application.Features.Auth.Commands;
using EcoBridge.Application.Services;
using EcoBridge.Application.UnitTests.Helpers;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Models.Accounts;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace EcoBridge.Application.UnitTests.Commands;

public class LoginCommandHandlerTests
{
    private readonly Mock<ITokenService> _tokenServiceMock = new();
    private readonly Mock<IRefreshTokenService> _refreshTokenServiceMock = new();
    private readonly IOptions<JWTSettings> _jwtOptions = Options.Create(new JWTSettings { DurationInMinutes = 60 });
    private readonly PasswordHasher<Account> _passwordHasher = new();

    [Fact]
    public async Task Handle_Should_Return_Unauthorized_When_Account_Not_Found()
    {
        // Arrange
        using var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var handler = new LoginCommandHandler(
            dbContext,
            _tokenServiceMock.Object,
            _refreshTokenServiceMock.Object,
            _jwtOptions);

        var command = new LoginCommand(new LoginRequestDTO
        {
            Email = "nonexistent@example.com",
            Password = "Password123!"
        });

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.Unauthorized);
        result.Message.Should().Be(AppMessageCodes.INVALID_CREDENTIALS);
    }

    [Fact]
    public async Task Handle_Should_Return_Forbidden_When_Account_Is_Deactivated()
    {
        // Arrange
        using var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var account = Account.Create("Deactivated User", "inactive@example.com", "010", TestPasswordHelper.Hash(), UserRole.Donor);
        account.Deactivate();
        dbContext.Accounts.Add(account);
        await dbContext.SaveChangesAsync();

        var handler = new LoginCommandHandler(
            dbContext,
            _tokenServiceMock.Object,
            _refreshTokenServiceMock.Object,
            _jwtOptions);

        var command = new LoginCommand(new LoginRequestDTO
        {
            Email = "inactive@example.com",
            Password = "Password123!"
        });

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.Forbidden);
        result.Message.Should().Be(AppMessageCodes.ACCOUNT_DEACTIVATED);
    }

    [Fact]
    public async Task Handle_Should_Return_Unauthorized_When_Password_Is_Incorrect()
    {
        // Arrange
        using var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var account = Account.Create("Active User", "user@example.com", "010", TestPasswordHelper.Hash("CorrectPassword123!"), UserRole.Donor);
        dbContext.Accounts.Add(account);
        await dbContext.SaveChangesAsync();

        var handler = new LoginCommandHandler(
            dbContext,
            _tokenServiceMock.Object,
            _refreshTokenServiceMock.Object,
            _jwtOptions);

        var command = new LoginCommand(new LoginRequestDTO
        {
            Email = "user@example.com",
            Password = "WrongPassword!"
        });

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.Unauthorized);
        result.Message.Should().Be(AppMessageCodes.INVALID_CREDENTIALS);
    }

    [Fact]
    public async Task Handle_Should_Return_Auth_Tokens_When_Credentials_Valid()
    {
        // Arrange
        using var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var account = Account.Create("John Donor", "john@donor.com", "010", TestPasswordHelper.Hash("ValidPassword123!"), UserRole.Donor);
        dbContext.Accounts.Add(account);
        await dbContext.SaveChangesAsync();

        _tokenServiceMock
            .Setup(t => t.GenerateAccessToken(It.IsAny<Account>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("valid.access.jwt");

        _refreshTokenServiceMock
            .Setup(r => r.GenerateAndSaveRefreshTokenAsync(account.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync("valid-refresh-token");

        var handler = new LoginCommandHandler(
            dbContext,
            _tokenServiceMock.Object,
            _refreshTokenServiceMock.Object,
            _jwtOptions);

        var command = new LoginCommand(new LoginRequestDTO
        {
            Email = "john@donor.com",
            Password = "ValidPassword123!"
        });

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Be(AppMessageCodes.LOGIN_SUCCESS);
        result.Value.AccessToken.Should().Be("valid.access.jwt");
        result.Value.RefreshToken.Should().Be("valid-refresh-token");
        result.Value.AccessTokenExpiresAtUtc.Should().BeAfter(DateTime.UtcNow);
    }
}
