using System;
using System.Threading;
using System.Threading.Tasks;
using EcoBridge.Application.Features.Events;
using EcoBridge.Application.Features.Missions.Commands;
using EcoBridge.Application.UnitTests.Helpers;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Models.Accounts;
using EcoBridge.Domain.Models.Donation;
using EcoBridge.Domain.Models.Mission;
using FluentAssertions;
using MediatR;
using Moq;
using Xunit;

namespace EcoBridge.Application.UnitTests.Commands;

public class ConfirmPickupCommandHandlerTests
{
    private readonly Mock<IPublisher> _publisherMock = new();

    [Fact]
    public async Task Handle_Should_Return_NotFound_When_Mission_Does_Not_Exist()
    {
        // Arrange
        using var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var handler = new ConfirmPickupCommandHandler(dbContext, _publisherMock.Object);
        var command = new ConfirmPickupCommand(Guid.NewGuid(), Guid.NewGuid());

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.NotFound);
        result.Message.Should().Be(AppMessageCodes.MISSION_NOT_FOUND);
    }

    [Fact]
    public async Task Handle_Should_Return_Forbidden_When_Caller_Is_Not_Donor()
    {
        // Arrange
        using var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var realDonor = Account.Create("Donor", "d@test.com", "010", TestPasswordHelper.Hash(), UserRole.Donor);
        var hacker = Account.Create("Hacker", "h@test.com", "010", TestPasswordHelper.Hash(), UserRole.Donor);

        var donation = Donation.Create(realDonor.Id, "Food", "10", DateTime.UtcNow.AddDays(1), "Pickup");
        var request = DonationRequest.Create(donation.Id, Guid.NewGuid());
        request.Donation = donation;

        var mission = DeliveryMission.Create(request.Id, "Pickup", "Dropoff");
        mission.Accept(Guid.NewGuid());
        mission.DonationRequest = request;

        dbContext.Accounts.AddRange(realDonor, hacker);
        dbContext.Donations.Add(donation);
        dbContext.DonationRequests.Add(request);
        dbContext.DeliveryMissions.Add(mission);
        await dbContext.SaveChangesAsync();

        var handler = new ConfirmPickupCommandHandler(dbContext, _publisherMock.Object);
        var command = new ConfirmPickupCommand(mission.Id, hacker.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.Forbidden);
        result.Message.Should().Be(AppMessageCodes.FORBIDDEN);
    }

    [Fact]
    public async Task Handle_Should_Confirm_Pickup_Successfully_And_Publish_MissionInTransitEvent()
    {
        // Arrange
        using var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var donor = Account.Create("Donor", "d@test.com", "010", TestPasswordHelper.Hash(), UserRole.Donor);
        var volunteer = Account.Create("Volunteer", "v@test.com", "010", TestPasswordHelper.Hash(), UserRole.Volunteer);
        var charity = Account.Create("Charity", "c@test.com", "010", TestPasswordHelper.Hash(), UserRole.Charity);

        var donation = Donation.Create(donor.Id, "Food", "10", DateTime.UtcNow.AddDays(1), "Pickup");
        var request = DonationRequest.Create(donation.Id, charity.Id);
        request.Donation = donation;

        var mission = DeliveryMission.Create(request.Id, "Pickup", "Dropoff");
        mission.Accept(volunteer.Id);
        mission.DonationRequest = request;

        dbContext.Accounts.AddRange(donor, volunteer, charity);
        dbContext.Donations.Add(donation);
        dbContext.DonationRequests.Add(request);
        dbContext.DeliveryMissions.Add(mission);
        await dbContext.SaveChangesAsync();

        var handler = new ConfirmPickupCommandHandler(dbContext, _publisherMock.Object);
        var command = new ConfirmPickupCommand(mission.Id, donor.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Be(AppMessageCodes.MISSION_PICKUP_CONFIRMED);

        var updated = await dbContext.DeliveryMissions.FindAsync(mission.Id);
        updated!.MissionStatus.Should().Be(MissionStatus.InTransit);
        updated.PickupTime.Should().NotBeNull();

        _publisherMock.Verify(p => p.Publish(
            It.Is<MissionInTransitEvent>(e => e.MissionId == mission.Id && e.VolunteerId == volunteer.Id),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }
}
