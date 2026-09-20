using System;
using System.Security.Claims;
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
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace EcoBridge.Application.UnitTests.Commands;

public class RefreshTokenCommandHandlerTests
{
    private readonly Mock<ITokenService> _tokenServiceMock = new();
    private readonly Mock<IRefreshTokenService> _refreshTokenServiceMock = new();
    private readonly IOptions<JWTSettings> _jwtOptions = Options.Create(new JWTSettings { DurationInMinutes = 60 });

    [Fact]
    public async Task Handle_Should_Return_TokenExpired_When_RefreshToken_Not_Found_In_Redis()
    {
        // Arrange
        using var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        _refreshTokenServiceMock
            .Setup(r => r.ValidateRefreshTokenAsync("expired-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid?)null);

        var handler = new RefreshTokenCommandHandler(
            dbContext,
            _tokenServiceMock.Object,
            _refreshTokenServiceMock.Object,
            _jwtOptions);

        var command = new RefreshTokenCommand(new RefreshTokenRequestDTO
        {
            AccessToken = "expired.access.jwt",
            RefreshToken = "expired-token"
        });

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be(ErrorCodes.TOKEN_EXPIRED);
    }

    [Fact]
    public async Task Handle_Should_Rotate_Tokens_Successfully_When_Valid()
    {
        // Arrange
        using var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var account = Account.Create("User", "user@test.com", "010", TestPasswordHelper.Hash(), UserRole.Volunteer);
        dbContext.Accounts.Add(account);
        await dbContext.SaveChangesAsync();

        _refreshTokenServiceMock
            .Setup(r => r.ValidateRefreshTokenAsync("valid-refresh", It.IsAny<CancellationToken>()))
            .ReturnsAsync(account.Id);

        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, account.Id.ToString())
        }));

        _tokenServiceMock
            .Setup(t => t.GetPrincipalFromExpiredToken("old.access.jwt"))
            .Returns(claimsPrincipal);

        _refreshTokenServiceMock
            .Setup(r => r.GenerateAndSaveRefreshTokenAsync(account.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync("brand-new-refresh-token");

        _tokenServiceMock
            .Setup(t => t.GenerateAccessToken(account, It.IsAny<CancellationToken>()))
            .ReturnsAsync("brand.new.access.jwt");

        var handler = new RefreshTokenCommandHandler(
            dbContext,
            _tokenServiceMock.Object,
            _refreshTokenServiceMock.Object,
            _jwtOptions);

        var command = new RefreshTokenCommand(new RefreshTokenRequestDTO
        {
            AccessToken = "old.access.jwt",
            RefreshToken = "valid-refresh"
        });

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.AccessToken.Should().Be("brand.new.access.jwt");
        result.Value.RefreshToken.Should().Be("brand-new-refresh-token");

        _refreshTokenServiceMock.Verify(r => r.RevokeRefreshTokenAsync("valid-refresh", It.IsAny<CancellationToken>()), Times.Once);
    }
}
