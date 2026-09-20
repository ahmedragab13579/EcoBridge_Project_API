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

public class CancelDonationCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_Return_NotFound_When_Donation_Does_Not_Exist()
    {
        // Arrange
        using var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var handler = new CancelDonationCommandHandler(dbContext);
        var command = new CancelDonationCommand(Guid.NewGuid(), Guid.NewGuid(), "Cancelled by donor");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.NotFound);
        result.Message.Should().Be(AppMessageCodes.DONATION_NOT_FOUND);
    }

    [Fact]
    public async Task Handle_Should_Return_Forbidden_When_Caller_Is_Not_Owner_Or_Admin()
    {
        // Arrange
        using var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var donorAccount = Account.Create("Donor", "donor@test.com", "010", TestPasswordHelper.Hash(), UserRole.Donor);
        var otherAccount = Account.Create("Other", "other@test.com", "010", TestPasswordHelper.Hash(), UserRole.Donor);

        var donation = Donation.Create(donorAccount.Id, "Food", "10", DateTime.UtcNow.AddDays(1), "Loc");
        donation.Donor = donorAccount;

        dbContext.Accounts.AddRange(donorAccount, otherAccount);
        dbContext.Donations.Add(donation);
        await dbContext.SaveChangesAsync();

        var handler = new CancelDonationCommandHandler(dbContext);
        var command = new CancelDonationCommand(donation.Id, otherAccount.Id, "Trying to cancel other's donation");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.Forbidden);
        result.Message.Should().Be(AppMessageCodes.FORBIDDEN);
    }

    [Fact]
    public async Task Handle_Should_Return_Conflict_When_Donation_Already_Delivered_Or_Cancelled()
    {
        // Arrange
        using var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var donorAccount = Account.Create("Donor", "donor@test.com", "010", TestPasswordHelper.Hash(), UserRole.Donor);

        var donation = Donation.Create(donorAccount.Id, "Food", "10", DateTime.UtcNow.AddDays(1), "Loc");
        donation.Donor = donorAccount;
        donation.MarkAsDelivered();

        dbContext.Accounts.Add(donorAccount);
        dbContext.Donations.Add(donation);
        await dbContext.SaveChangesAsync();

        var handler = new CancelDonationCommandHandler(dbContext);
        var command = new CancelDonationCommand(donation.Id, donorAccount.Id, "Cancel delivered");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.Conflict);
        result.Message.Should().Be(AppMessageCodes.DONATION_ALREADY_CANCELLED);
    }

    [Fact]
    public async Task Handle_Should_Cancel_Donation_Successfully_When_Valid()
    {
        // Arrange
        using var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var donorAccount = Account.Create("Donor", "donor@test.com", "010", TestPasswordHelper.Hash(), UserRole.Donor);

        var donation = Donation.Create(donorAccount.Id, "Meals", "25", DateTime.UtcNow.AddDays(1), "Restaurant A");
        donation.Donor = donorAccount;

        dbContext.Accounts.Add(donorAccount);
        dbContext.Donations.Add(donation);
        await dbContext.SaveChangesAsync();

        var handler = new CancelDonationCommandHandler(dbContext);
        var command = new CancelDonationCommand(donation.Id, donorAccount.Id, "Restaurant closed early");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Be(AppMessageCodes.DONATION_CANCELLED);

        var updated = await dbContext.Donations.FindAsync(donation.Id);
        updated!.Status.Should().Be(DonationStatus.Cancelled);
        updated.CancellationReason.Should().Be("Restaurant closed early");
    }
}
