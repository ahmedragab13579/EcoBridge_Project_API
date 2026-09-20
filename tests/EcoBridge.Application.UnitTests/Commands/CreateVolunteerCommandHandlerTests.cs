using System;
using System.Threading;
using System.Threading.Tasks;
using EcoBridge.Application.Contracts;
using EcoBridge.Application.DTO;
using EcoBridge.Application.Features.Volunteer.Commands;
using EcoBridge.Application.UnitTests.Helpers;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Models.Accounts;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Xunit;

namespace EcoBridge.Application.UnitTests.Commands;

public class CreateVolunteerCommandHandlerTests
{
        private readonly WebApplicationFactory<Program> _factory = new WebApplicationFactory<Program>();

    [Fact]
    public async Task Handle_Should_Return_NotFound_When_Account_Does_Not_Exist()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var myService = scope.ServiceProvider.GetRequiredService<IPasswordHasher<Account>>();
        var myService2 = scope.ServiceProvider.GetRequiredService<IEmailService>();

        using var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var handler = new CreateVolunteerCommandHandler(dbContext,myService,myService2);
        var command = new CreateVolunteerCommand(Guid.NewGuid(), new CreateVolunteerDTO { VehicleDetails = "Car" });

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.NotFound);
        result.Message.Should().Be(AppMessageCodes.ACCOUNT_NOT_FOUND);
    }

    [Fact]
    public async Task Handle_Should_Return_Conflict_When_Volunteer_Profile_Already_Exists()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var myService = scope.ServiceProvider.GetRequiredService<IPasswordHasher<Account>>();
        var myService2 = scope.ServiceProvider.GetRequiredService<IEmailService>();

        using var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var account = Account.Create("Volunteer User", "vol@test.com", "010", TestPasswordHelper.Hash(), UserRole.Volunteer);
        account.UpdateVehicle("Scooter","");

        dbContext.Accounts.Add(account);
        await dbContext.SaveChangesAsync();

        var handler = new CreateVolunteerCommandHandler(dbContext, myService, myService2);
        var command = new CreateVolunteerCommand(account.Id, new CreateVolunteerDTO { VehicleDetails = "Scooter" });

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.Conflict);
        result.Message.Should().Be(AppMessageCodes.CONFLICT);
    }

    [Fact]
    public async Task Handle_Should_Create_Volunteer_Profile_Successfully()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var myService = scope.ServiceProvider.GetRequiredService<IPasswordHasher<Account>>();
        var myService2 = scope.ServiceProvider.GetRequiredService<IEmailService>();

        using var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var account = Account.Create("Volunteer User", "vol2@test.com", "010", TestPasswordHelper.Hash(), UserRole.Volunteer);
        dbContext.Accounts.Add(account);
        await dbContext.SaveChangesAsync();

        var handler = new CreateVolunteerCommandHandler(dbContext, myService, myService2);
        var command = new CreateVolunteerCommand(account.Id, new CreateVolunteerDTO { VehicleDetails = "Toyota Yaris" });

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Be(AppMessageCodes.VOLUNTEER_CREATED);
        result.Value.Should().Be(account.Id);

        var saved = await dbContext.Accounts.FindAsync(account.Id);
        saved.Should().NotBeNull();
        saved!.VehicleDetails.Should().Be("Toyota Yaris");
        saved.IsAvailable.Should().BeTrue();
    }
}
