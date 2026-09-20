using System;
using System.Threading;
using System.Threading.Tasks;
using EcoBridge.Application.Features.Admin.Commands;
using EcoBridge.Application.UnitTests.Helpers;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Models.Accounts;
using FluentAssertions;
using Xunit;

namespace EcoBridge.Application.UnitTests.Commands;

public class ToggleUserStatusCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_Return_NotFound_When_User_Does_Not_Exist()
    {
        // Arrange
        using var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var handler = new ToggleUserStatusCommandHandler(dbContext);
        var command = new ToggleUserStatusCommand(Guid.NewGuid());

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.NotFound);
        result.Message.Should().Be(AppMessageCodes.USER_NOT_FOUND);
    }

    [Fact]
    public async Task Handle_Should_Deactivate_Active_User()
    {
        // Arrange
        using var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var account = Account.Create("Active User", "active@test.com", "010", TestPasswordHelper.Hash(), UserRole.Volunteer);
        dbContext.Accounts.Add(account);
        await dbContext.SaveChangesAsync();

        var handler = new ToggleUserStatusCommandHandler(dbContext);
        var command = new ToggleUserStatusCommand(account.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Be(AppMessageCodes.USER_STATUS_UPDATED);

        var updated = await dbContext.Accounts.FindAsync(account.Id);
        updated!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_Should_Activate_Deactivated_User()
    {
        // Arrange
        using var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var account = Account.Create("Inactive User", "inactive@test.com", "010", TestPasswordHelper.Hash(), UserRole.Volunteer);
        account.Deactivate();
        dbContext.Accounts.Add(account);
        await dbContext.SaveChangesAsync();

        var handler = new ToggleUserStatusCommandHandler(dbContext);
        var command = new ToggleUserStatusCommand(account.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Be(AppMessageCodes.USER_STATUS_UPDATED);

        var updated = await dbContext.Accounts.FindAsync(account.Id);
        updated!.IsActive.Should().BeTrue();
    }
}
