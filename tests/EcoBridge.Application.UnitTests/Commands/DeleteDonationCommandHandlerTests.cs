using System;
using System.Threading;
using System.Threading.Tasks;
using EcoBridge.Application.Features.Donation.Commands;
using EcoBridge.Application.UnitTests.Helpers;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Models.Accounts;
using EcoBridge.Domain.Models.Donation;
using FluentAssertions;
using Xunit;

namespace EcoBridge.Application.UnitTests.Commands;

public class DeleteDonationCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_Return_NotFound_When_Donor_Does_Not_Exist()
    {
        // Arrange
        using var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var handler = new DeleteDonationCommandHandler(dbContext);
        var command = new DeleteDonationCommand(Guid.NewGuid(), Guid.NewGuid());

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.NotFound);
        result.Message.Should().Be(AppMessageCodes.DONOR_NOT_FOUND);
    }

    [Fact]
    public async Task Handle_Should_Return_Forbidden_When_Caller_Is_Not_Owner_Donor()
    {
        // Arrange
        using var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var donor1Account = Account.Create("Donor 1", "d1@test.com", "010", TestPasswordHelper.Hash(), UserRole.Donor);
        var donor2Account = Account.Create("Donor 2", "d2@test.com", "010", TestPasswordHelper.Hash(), UserRole.Donor);

        dbContext.Accounts.AddRange(donor1Account, donor2Account);
        dbContext.Donations.Add(donation);
        await dbContext.SaveChangesAsync();

        var handler = new DeleteDonationCommandHandler(dbContext);
        var command = new DeleteDonationCommand(donation.Id, donor2Account.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.Forbidden);
        result.Message.Should().Be(AppMessageCodes.FORBIDDEN);
    }

    [Fact]
    public async Task Handle_Should_SoftDelete_Donation_Successfully()
    {
        // Arrange
        using var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var donorAccount = Account.Create("Donor", "donor@test.com", "010", TestPasswordHelper.Hash(), UserRole.Donor);
        var donor = Donor.Create(donorAccount.Id, UserType.Individual);
        var donation = Donation.Create(donorAccount.Id, "Pastries", "30", DateTime.UtcNow.AddDays(1), "Bakery");

        dbContext.Accounts.Add(donorAccount);
        dbContext.Donors.Add(donor);
        dbContext.Donations.Add(donation);
        await dbContext.SaveChangesAsync();

        var handler = new DeleteDonationCommandHandler(dbContext);
        var command = new DeleteDonationCommand(donation.Id, donorAccount.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Be(AppMessageCodes.DONATION_DELETED);

        var deleted = await dbContext.Donations.FindAsync(donation.Id);
        deleted!.IsDeleted.Should().BeTrue();
    }
}
