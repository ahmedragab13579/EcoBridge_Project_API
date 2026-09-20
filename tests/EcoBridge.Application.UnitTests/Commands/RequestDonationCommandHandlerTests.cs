using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EcoBridge.Application.Features.Donation.Commands;
using EcoBridge.Application.UnitTests.Helpers;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Models.Accounts;
using EcoBridge.Domain.Models.Donation;
using FluentAssertions;
using Hangfire;
using Moq;
using Xunit;

namespace EcoBridge.Application.UnitTests.Commands;

public class RequestDonationCommandHandlerTests
{
    public RequestDonationCommandHandlerTests()
    {
        JobStorage.Current = new Mock<JobStorage>().Object;
    }

    [Fact]
    public async Task Handle_Should_Return_NotFound_When_Charity_Does_Not_Exist()
    {
        // Arrange
        using var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var mockJobClient = new Mock<IBackgroundJobClient>();
        var handler = new RequestDonationCommandHandler(mockJobClient.Object,dbContext);
        var command = new RequestDonationCommand(Guid.NewGuid(), Guid.NewGuid());

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.NotFound);
        result.Message.Should().Be(AppMessageCodes.CHARITY_NOT_FOUND);
    }

    [Fact]
    public async Task Handle_Should_Return_NotFound_When_Donation_Does_Not_Exist()
    {
        // Arrange
        using var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var charityAccount = Account.Create("Charity Org", "charity@test.com", "010", TestPasswordHelper.Hash(), UserRole.Charity);
        var charity = Charity.Create(charityAccount.Id, "Charity Org", "REG-999");
        charity.Account = charityAccount;

        dbContext.Accounts.Add(charityAccount);
        dbContext.Charities.Add(charity);
        await dbContext.SaveChangesAsync();
        var mockJobClient = new Mock<IBackgroundJobClient>();
        var handler = new RequestDonationCommandHandler(mockJobClient.Object,dbContext);
        var command = new RequestDonationCommand(Guid.NewGuid(), charityAccount.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.NotFound);
        result.Message.Should().Be(AppMessageCodes.DONATION_NOT_FOUND);
    }

    [Fact]
    public async Task Handle_Should_Return_Conflict_When_Donation_Is_Already_Requested()
    {
        // Arrange
        using var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var charity1Account = Account.Create("Charity 1", "c1@test.com", "010", TestPasswordHelper.Hash(), UserRole.Charity);
        var charity2Account = Account.Create("Charity 2", "c2@test.com", "010", TestPasswordHelper.Hash(), UserRole.Charity);
        var donorAccount = Account.Create("Donor", "donor@test.com", "010", TestPasswordHelper.Hash(), UserRole.Donor);

        var c1 = Charity.Create(charity1Account.Id, "Charity 1", "REG-1");
        c1.Account = charity1Account;
        var c2 = Charity.Create(charity2Account.Id, "Charity 2", "REG-2");
        c2.Account = charity2Account;
        var donor = Donor.Create(donorAccount.Id, UserType.Individual);
        donor.Account = donorAccount;

        var donation = Donation.Create(donorAccount.Id, "Apples", "10 kg", DateTime.UtcNow.AddDays(1), "Farm 1");
        donation.Donor = donor;
        donation.MarkAsAccepted(c1.AccountId); // Already claimed

        dbContext.Accounts.AddRange(charity1Account, charity2Account, donorAccount);
        dbContext.Donors.Add(donor);
        dbContext.Charities.AddRange(c1, c2);
        dbContext.Donations.Add(donation);
        await dbContext.SaveChangesAsync();
        var mockJobClient = new Mock<IBackgroundJobClient>();
        var handler = new RequestDonationCommandHandler(mockJobClient.Object,dbContext);
        var command = new RequestDonationCommand(donation.Id, c2.AccountId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.Conflict);
        result.Message.Should().Be(AppMessageCodes.DONATION_ALREADY_REQUESTED);
    }

    [Fact]
    public async Task Handle_Should_Create_DonationRequest_Successfully()
    {
        // Arrange
        using var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var charityAccount = Account.Create("Charity A", "ca@test.com", "010", TestPasswordHelper.Hash(), UserRole.Charity);
        var donorAccount = Account.Create("Donor B", "db@test.com", "010", TestPasswordHelper.Hash(), UserRole.Donor);
        var charity = Charity.Create(charityAccount.Id, "Charity A", "REG-555");
        charity.Account = charityAccount;
        var donor = Donor.Create(donorAccount.Id, UserType.Individual);
        donor.Account = donorAccount;

        var donation = Donation.Create(donorAccount.Id, "Sandwiches", "50 boxes", DateTime.UtcNow.AddDays(1), "Deli 2");
        donation.Donor = donor;

        dbContext.Accounts.AddRange(charityAccount, donorAccount);
        dbContext.Donors.Add(donor);
        dbContext.Charities.Add(charity);
        dbContext.Donations.Add(donation);
        await dbContext.SaveChangesAsync();
        var mockJobClient = new Mock<IBackgroundJobClient>();
        var handler = new RequestDonationCommandHandler(mockJobClient.Object,dbContext);
        var command = new RequestDonationCommand(donation.Id, charityAccount.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Be(AppMessageCodes.DONATION_REQUEST_CREATED);
        result.Value.Should().NotBeEmpty();

        var savedRequest = dbContext.DonationRequests.FirstOrDefault(r => r.Id == result.Value);
        savedRequest.Should().NotBeNull();
        savedRequest!.DonationId.Should().Be(donation.Id);
        savedRequest.CharityId.Should().Be(charityAccount.Id);
        savedRequest.RequestStatus.Should().Be(RequestStatus.Pending);
    }
}
