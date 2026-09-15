using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EcoBridge.Application.Contracts;
using EcoBridge.Application.Features.Donation.Commands;
using EcoBridge.Application.Features.Events;
using EcoBridge.Application.UnitTests.Helpers;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Models;
using FluentAssertions;
using MediatR;
using Moq;
using Xunit;

namespace EcoBridge.Application.UnitTests.Commands;

public class ApproveDonationRequestCommandHandlerTests
{
    private readonly Mock<IPublisher> _publisherMock = new();

    [Fact]
    public async Task Handle_Should_Return_NotFound_When_DonationRequest_Does_Not_Exist()
    {
        // Arrange
        using var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var handler = new ApproveDonationRequestCommandHandler(dbContext, _publisherMock.Object);

        var command = new ApproveDonationRequestCommand(Guid.NewGuid(), Guid.NewGuid());

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.NotFound);
        result.Message.Should().Be(AppMessageCodes.DONATION_REQUEST_NOT_FOUND);
    }

    [Fact]
    public async Task Handle_Should_Return_Forbidden_When_Caller_Is_Not_Donor()
    {
        // Arrange
        using var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var realDonorAccount = Account.Create("Real Donor", "donor@test.com", "010", TestPasswordHelper.Hash(), UserRole.Donor);
        var realDonor = Donor.Create(realDonorAccount.Id, UserType.Individual);
        realDonor.Account = realDonorAccount;

        var hackerAccount = Account.Create("Other User", "other@test.com", "010", TestPasswordHelper.Hash(), UserRole.Donor);
        var hackerDonor = Donor.Create(hackerAccount.Id, UserType.Individual);
        hackerDonor.Account = hackerAccount;

        var charityAccount = Account.Create("Charity", "charity@test.com", "010", TestPasswordHelper.Hash(), UserRole.Charity);
        var charity = Charity.Create(charityAccount.Id, "Charity", "REG-123");
        charity.Account = charityAccount;

        var donation = Donation.Create(realDonorAccount.Id, "Food", "10", DateTime.UtcNow.AddDays(1), "Loc");
        donation.Status = DonationStatus.Pending;
        donation.Donor = realDonor;

        var donationRequest = DonationRequest.Create(donation.Id, charityAccount.Id);
        donationRequest.Donation = donation;
        donationRequest.Charity = charity;

        dbContext.Accounts.AddRange(realDonorAccount, hackerAccount, charityAccount);
        dbContext.Donors.AddRange(realDonor, hackerDonor);
        dbContext.Charities.Add(charity);
        dbContext.Donations.Add(donation);
        dbContext.DonationRequests.Add(donationRequest);
        await dbContext.SaveChangesAsync();

        var handler = new ApproveDonationRequestCommandHandler(dbContext, _publisherMock.Object);
        var command = new ApproveDonationRequestCommand(donationRequest.Id, hackerAccount.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.Forbidden);
        result.Message.Should().Be(AppMessageCodes.FORBIDDEN);
    }

    [Fact]
    public async Task Handle_Should_Approve_Request_Create_DeliveryMission_And_Publish_Event()
    {
        // Arrange
        using var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var donorAccount = Account.Create("Donor", "donor@ecobridge.org", "010", TestPasswordHelper.Hash(), UserRole.Donor);
        var donor = Donor.Create(donorAccount.Id, UserType.Individual);
        donor.Account = donorAccount;

        var charityAccount = Account.Create("Red Crescent", "charity@ecobridge.org", "010", TestPasswordHelper.Hash(), UserRole.Charity);
        var charity = Charity.Create(charityAccount.Id, "Red Crescent", "Reg123");
        charity.Account = charityAccount;

        var donation = Donation.Create(donorAccount.Id, "Rice", "50 bags", DateTime.UtcNow.AddDays(2), "Warehouse 4");
        donation.Status = DonationStatus.Pending;
        donation.Donor = donor;

        var donationRequest = DonationRequest.Create(donation.Id, charity.AccountId);
        donationRequest.Donation = donation;
        donationRequest.Charity = charity;

        dbContext.Accounts.AddRange(donorAccount, charityAccount);
        dbContext.Donors.Add(donor);
        dbContext.Charities.Add(charity);
        dbContext.Donations.Add(donation);
        dbContext.DonationRequests.Add(donationRequest);
        await dbContext.SaveChangesAsync();

        var handler = new ApproveDonationRequestCommandHandler(dbContext, _publisherMock.Object);
        var command = new ApproveDonationRequestCommand(donationRequest.Id, donorAccount.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Be(AppMessageCodes.DONATION_REQUEST_APPROVED);
        result.Value.Should().NotBeEmpty(); // MissionId

        // Verify entities modified
        donationRequest.RequestStatus.Should().Be(RequestStatus.ApprovedByDonor);
        donation.Status.Should().Be(DonationStatus.SearchingForVolunteer);

        var mission = dbContext.DeliveryMissions.FirstOrDefault(m => m.Id == result.Value);
        mission.Should().NotBeNull();
        mission!.MissionStatus.Should().Be(MissionStatus.SearchingForVolunteer);
        mission.OtpCode.Should().NotBeNullOrWhiteSpace();

        // Verify event published
        _publisherMock.Verify(p => p.Publish(
            It.Is<MissionCreatedEvent>(e => e.MissionId == result.Value && e.DonationId == donation.Id),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }
}
