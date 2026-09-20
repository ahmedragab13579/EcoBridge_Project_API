using System;
using System.Threading;
using System.Threading.Tasks;
using EcoBridge.Application.Features.Volunteer.Commands;
using EcoBridge.Application.UnitTests.Helpers;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Models.Accounts;
using FluentAssertions;
using Xunit;

namespace EcoBridge.Application.UnitTests.Commands;

public class ToggleVolunteerAvailabilityCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_Return_NotFound_When_Volunteer_Does_Not_Exist()
    {
        // Arrange
        using var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var handler = new ToggleVolunteerAvailabilityCommandHandler(dbContext);
        var command = new ToggleVolunteerAvailabilityCommand(Guid.NewGuid(), false);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.NotFound);
        result.Message.Should().Be(AppMessageCodes.VOLUNTEER_NOT_FOUND);
    }

    [Fact]
    public async Task Handle_Should_Toggle_Availability_Successfully()
    {
        // Arrange
        using var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var account = Account.Create("Volunteer User", "vol@test.com", "010", TestPasswordHelper.Hash(), UserRole.Volunteer);
        var volunteer = Volunteer.Create(account.Id, "Car");
        volunteer.IsAvailable = true;

        dbContext.Accounts.Add(account);
        dbContext.Volunteers.Add(volunteer);
        await dbContext.SaveChangesAsync();

        var handler = new ToggleVolunteerAvailabilityCommandHandler(dbContext);
        var command = new ToggleVolunteerAvailabilityCommand(account.Id, false);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Be(AppMessageCodes.VOLUNTEER_AVAILABILITY_UPDATED);

        var updated = await dbContext.Volunteers.FindAsync(account.Id);
        updated!.IsAvailable.Should().BeFalse();
    }
}
