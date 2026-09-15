using System;
using System.Threading;
using System.Threading.Tasks;
using EcoBridge.Application.DTO;
using EcoBridge.Application.Features.Auth.Commands;
using EcoBridge.Application.UnitTests.Helpers;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Xunit;

namespace EcoBridge.Application.UnitTests.Commands;

public class ChangeUserPasswordCommandHandlerTests
{
    private readonly PasswordHasher<Account> _passwordHasher = new();

    [Fact]
    public async Task Handle_Should_Return_NotFound_When_User_Does_Not_Exist()
    {
        // Arrange
        using var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var handler = new ChangeUserPasswordCommandHandler(dbContext);
        var command = new ChangeUserPasswordCommand(Guid.NewGuid(), new ChangePasswordDTO
        {
            CurrentPassword = "OldPassword123!",
            NewPassword = "NewPassword123!"
        });

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.NotFound);
        result.Message.Should().Be(AppMessageCodes.USER_NOT_FOUND);
    }

    [Fact]
    public async Task Handle_Should_Return_Unauthorized_When_CurrentPassword_Is_Incorrect()
    {
        // Arrange
        using var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var account = Account.Create("User", "user@test.com", "010", _passwordHasher.HashPassword(null!, "RealPassword123!"), UserRole.Donor);
        dbContext.Accounts.Add(account);
        await dbContext.SaveChangesAsync();

        var handler = new ChangeUserPasswordCommandHandler(dbContext);
        var command = new ChangeUserPasswordCommand(account.Id, new ChangePasswordDTO
        {
            CurrentPassword = "WrongPassword!",
            NewPassword = "NewPassword123!"
        });

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.Unauthorized);
        result.Message.Should().Be(AppMessageCodes.INVALID_CREDENTIALS);
    }

    [Fact]
    public async Task Handle_Should_Change_Password_Successfully_When_Valid()
    {
        // Arrange
        using var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var account = Account.Create("User", "user@test.com", "010", _passwordHasher.HashPassword(null!, "OldPassword123!"), UserRole.Donor);
        dbContext.Accounts.Add(account);
        await dbContext.SaveChangesAsync();

        var handler = new ChangeUserPasswordCommandHandler(dbContext);
        var command = new ChangeUserPasswordCommand(account.Id, new ChangePasswordDTO
        {
            CurrentPassword = "OldPassword123!",
            NewPassword = "NewPassword123!"
        });

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Be(AppMessageCodes.PASSWORD_CHANGE_SUCCESS);

        var updated = await dbContext.Accounts.FindAsync(account.Id);
        var verify = _passwordHasher.VerifyHashedPassword(updated!, updated!.PasswordHash ?? string.Empty, "NewPassword123!");
        verify.Should().Be(PasswordVerificationResult.Success);
    }
}
