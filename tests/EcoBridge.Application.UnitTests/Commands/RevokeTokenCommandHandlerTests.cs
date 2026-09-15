using System;
using System.Threading;
using System.Threading.Tasks;
using EcoBridge.Application.Features.Auth;
using EcoBridge.Application.Features.Auth.Commands;
using EcoBridge.Domain.Errors;
using FluentAssertions;
using Moq;
using Xunit;

namespace EcoBridge.Application.UnitTests.Commands;

public class RevokeTokenCommandHandlerTests
{
    private readonly Mock<IRefreshTokenService> _refreshTokenServiceMock = new();

    [Fact]
    public async Task Handle_Should_Revoke_All_Tokens_When_RevokeAll_Is_True()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var handler = new RevokeTokenCommandHandler(_refreshTokenServiceMock.Object);
        var command = new RevokeTokenCommand(null, userId, true );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Be(AppMessageCodes.LOGOUT_SUCCESS);
        _refreshTokenServiceMock.Verify(r => r.RevokeAllUserTokensAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Revoke_Specific_Token_When_Token_Provided()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var token = "some-refresh-token";
        _refreshTokenServiceMock
            .Setup(r => r.RevokeRefreshTokenAsync(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new RevokeTokenCommandHandler(_refreshTokenServiceMock.Object);
        var command = new RevokeTokenCommand(token, userId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Be(AppMessageCodes.LOGOUT_SUCCESS);
        _refreshTokenServiceMock.Verify(r => r.RevokeRefreshTokenAsync(token, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Return_Validation_Error_When_Input_Invalid()
    {
        // Arrange
        var handler = new RevokeTokenCommandHandler(_refreshTokenServiceMock.Object);
        var command = new RevokeTokenCommand("",null);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.Validation);
        result.Message.Should().Be(AppMessageCodes.INVALID_INPUT);
    }
}
