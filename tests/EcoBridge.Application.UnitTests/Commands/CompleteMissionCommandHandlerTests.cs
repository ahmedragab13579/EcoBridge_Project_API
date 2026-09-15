using System;
using System.Threading;
using System.Threading.Tasks;
using EcoBridge.Application.Features.Events;
using EcoBridge.Application.Features.Missions.Commands;
using EcoBridge.Application.UnitTests.Helpers;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Models;
using FluentAssertions;
using MediatR;
using Moq;
using Xunit;

namespace EcoBridge.Application.UnitTests.Commands;

public class CompleteMissionCommandHandlerTests
{
    private readonly Mock<IPublisher> _publisherMock = new();

    [Fact]
    public async Task Handle_Should_Return_NotFound_When_Mission_Does_Not_Exist()
    {
        // Arrange
        using var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var handler = new CompleteMissionCommandHandler(dbContext, _publisherMock.Object);
        var command = new CompleteMissionCommand(Guid.NewGuid(), Guid.NewGuid(), "1234");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.NotFound);
        result.Message.Should().Be(AppMessageCodes.MISSION_NOT_FOUND);
    }

    [Fact]
    public async Task Handle_Should_Return_Forbidden_When_Volunteer_Does_Not_Own_Mission()
    {
        // Arrange
        using var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var volunteer1 = Account.Create("Vol 1", "v1@test.com", "010", TestPasswordHelper.Hash(), UserRole.Volunteer);
        var volunteer2 = Account.Create("Vol 2", "v2@test.com", "010", TestPasswordHelper.Hash(), UserRole.Volunteer);
        var donor = Account.Create("Donor", "d@test.com", "010", TestPasswordHelper.Hash(), UserRole.Donor);
        var charity = Account.Create("Charity", "c@test.com", "010", TestPasswordHelper.Hash(), UserRole.Charity);

        var donation = Donation.Create(donor.Id, "Food", "10", DateTime.UtcNow.AddDays(1), "Pickup");
        var donationRequest = DonationRequest.Create(donation.Id, charity.Id);
        donationRequest.Donation = donation;

        var mission = DeliveryMission.Create(donationRequest.Id, "Pickup", "Dropoff", "1234", DateTime.UtcNow.AddHours(2));
        mission.Accept(volunteer1.Id);
        mission.DonationRequest = donationRequest;

        dbContext.Accounts.AddRange(volunteer1, volunteer2, donor, charity);
        dbContext.Donations.Add(donation);
        dbContext.DonationRequests.Add(donationRequest);
        dbContext.DeliveryMissions.Add(mission);
        await dbContext.SaveChangesAsync();

        var handler = new CompleteMissionCommandHandler(dbContext, _publisherMock.Object);
        var command = new CompleteMissionCommand(mission.Id, volunteer2.Id, "1234");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.Forbidden);
        result.Message.Should().Be(AppMessageCodes.FORBIDDEN);
    }

    [Fact]
    public async Task Handle_Should_Return_Validation_Error_When_Otp_Is_Incorrect()
    {
        // Arrange
        using var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var volunteer = Account.Create("Vol", "v@test.com", "010", TestPasswordHelper.Hash(), UserRole.Volunteer);
        var donor = Account.Create("Donor", "d@test.com", "010", TestPasswordHelper.Hash(), UserRole.Donor);
        var charity = Account.Create("Charity", "c@test.com", "010", TestPasswordHelper.Hash(), UserRole.Charity);

        var donation = Donation.Create(donor.Id, "Food", "10", DateTime.UtcNow.AddDays(1), "Pickup");
        var donationRequest = DonationRequest.Create(donation.Id, charity.Id);
        donationRequest.Donation = donation;

        var mission = DeliveryMission.Create(donationRequest.Id, "Pickup", "Dropoff", "9999", DateTime.UtcNow.AddHours(2));
        mission.Accept(volunteer.Id);
        mission.ConfirmPickup();
        mission.DonationRequest = donationRequest;

        dbContext.Accounts.AddRange(volunteer, donor, charity);
        dbContext.Donations.Add(donation);
        dbContext.DonationRequests.Add(donationRequest);
        dbContext.DeliveryMissions.Add(mission);
        await dbContext.SaveChangesAsync();

        var handler = new CompleteMissionCommandHandler(dbContext, _publisherMock.Object);
        var command = new CompleteMissionCommand(mission.Id, volunteer.Id, "0000");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.Validation);
        result.Message.Should().Be(AppMessageCodes.INVALID_OTP);
    }

    [Fact]
    public async Task Handle_Should_Complete_Mission_Successfully_And_Publish_DeliveryCompletedEvent()
    {
        // Arrange
        using var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var donor = Account.Create("Donor", "d@test.com", "010", TestPasswordHelper.Hash(), UserRole.Donor);
        var charity = Account.Create("Charity", "c@test.com", "010", TestPasswordHelper.Hash(), UserRole.Charity);
        var volunteer = Account.Create("Vol", "v@test.com", "010", TestPasswordHelper.Hash(), UserRole.Volunteer);

        var donation = Donation.Create(donor.Id, "Food", "10", DateTime.UtcNow.AddDays(1), "Pickup");
        var donationRequest = DonationRequest.Create(donation.Id, charity.Id);
        donationRequest.Donation = donation;

        var mission = DeliveryMission.Create(donationRequest.Id, "Pickup", "Dropoff", "5544", DateTime.UtcNow.AddHours(2));
        mission.Accept(volunteer.Id);
        mission.ConfirmPickup();
        mission.DonationRequest = donationRequest;

        dbContext.Accounts.AddRange(donor, charity, volunteer);
        dbContext.Donations.Add(donation);
        dbContext.DonationRequests.Add(donationRequest);
        dbContext.DeliveryMissions.Add(mission);
        await dbContext.SaveChangesAsync();

        var handler = new CompleteMissionCommandHandler(dbContext, _publisherMock.Object);
        var command = new CompleteMissionCommand(mission.Id, volunteer.Id, "5544");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Be(AppMessageCodes.MISSION_COMPLETED);

        var updated = await dbContext.DeliveryMissions.FindAsync(mission.Id);
        updated!.MissionStatus.Should().Be(MissionStatus.Delivered);
        updated.DeliveryTime.Should().NotBeNull();

        _publisherMock.Verify(p => p.Publish(
            It.Is<DeliveryCompletedEvent>(e => e.MissionId == mission.Id && e.VolunteerId == volunteer.Id),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }
}
