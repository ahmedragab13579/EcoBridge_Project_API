using System;
using System.Threading;
using System.Threading.Tasks;
using EcoBridge.Application.Features.Donation.Commands;
using EcoBridge.Application.UnitTests.Helpers;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Exceptions;
using EcoBridge.Domain.Models.Accounts;
using EcoBridge.Domain.Models.Donation;
using FluentAssertions;
using Xunit;

namespace EcoBridge.Application.UnitTests.Commands;

public class ClaimDonationCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_Claim_Available_Donation_For_Existing_Charity()
    {
        using var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var donation = CreateDonation();
        var charity = Account.Create(
            "Charity",
            "charity@test.com",
            "011",
            TestPasswordHelper.Hash(),
            UserRole.Charity,
            UserType.Company,
            organizationName: "Community Charity",
            registrationNumber: "REG-1");
        dbContext.Donations.Add(donation);
        dbContext.Accounts.Add(charity);
        await dbContext.SaveChangesAsync();

        var result = await new ClaimDonationCommandHandler(dbContext)
            .Handle(new ClaimDonationCommand(donation.Id, charity.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Code.Should().Be(AppMessageCodes.DONATION_ACCEPTED);
        donation.CharityId.Should().Be(charity.Id);
        donation.Status.Should().Be(DonationStatus.SearchingForVolunteer);
    }

    [Fact]
    public async Task Handle_Should_Throw_NotFound_When_Charity_Does_Not_Exist()
    {
        using var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var donation = CreateDonation();
        dbContext.Donations.Add(donation);
        await dbContext.SaveChangesAsync();

        var action = () => new ClaimDonationCommandHandler(dbContext).Handle(
            new ClaimDonationCommand(donation.Id, Guid.NewGuid()),
            CancellationToken.None);

        var exception = await action.Should().ThrowAsync<DomainException.NotFoundException>();
        exception.Which.Code.Should().Be(AppMessageCodes.CHARITY_NOT_FOUND);
    }

    private static Donation CreateDonation()
    {
        return Donation.Create(
            Guid.NewGuid(),
            "Bakery",
            "10 boxes",
            DateTime.UtcNow.AddDays(1),
            "Pickup location",
            1,
            "Fresh food",
            "Food description",
            1);
    }
}
