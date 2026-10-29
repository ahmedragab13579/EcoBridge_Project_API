using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EcoBridge.Application.Contracts;
using EcoBridge.Application.DTO.WorkFlow;
using EcoBridge.Application.Features.Donation.Commands;
using EcoBridge.Application.Features.Donation.Handler;
using EcoBridge.Application.Result;
using EcoBridge.Application.UnitTests.Helpers;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Exceptions;
using EcoBridge.Domain.Models.Accounts;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace EcoBridge.Application.UnitTests.Commands;

public class CreateDonationCommandHandlerTests
{
    private readonly Mock<IPhotoService> _photoServiceMock = new();

    [Fact]
    public async Task Handle_Should_Throw_NotFound_When_Donor_Does_Not_Exist()
    {
        using var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var handler = new CreateDonationCommandHandler(dbContext, _photoServiceMock.Object);
        var command = new CreateDonationCommand(Guid.NewGuid(), CreateDonationDto());

        var action = () => handler.Handle(command, CancellationToken.None);

        var exception = await action.Should().ThrowAsync<DomainException.NotFoundException>();
        exception.Which.Code.Should().Be(AppMessageCodes.DONOR_NOT_FOUND);
    }

    [Fact]
    public async Task Handle_Should_Create_Donation_Successfully_Without_Image()
    {
        using var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var account = Account.Create("Donor Name", "donor@test.com", "010", TestPasswordHelper.Hash(), UserRole.Donor);
        dbContext.Accounts.Add(account);
        await dbContext.SaveChangesAsync();
        var handler = new CreateDonationCommandHandler(dbContext, _photoServiceMock.Object);

        var result = await handler.Handle(
            new CreateDonationCommand(account.Id, CreateDonationDto()),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Code.Should().Be(AppMessageCodes.DONATION_CREATED);
        result.Data.Should().NotBeNull();
        result.Data!.Title.Should().Be("Fresh Bread");
        result.Data.FoodType.Should().Be("Bakery");
        result.Data.QuantityDes.Should().Be("20 loaves");
        result.Data.Status.Should().Be(DonationStatus.Available.ToString());
        result.Data.DonorId.Should().Be(account.Id);

        var saved = dbContext.Donations.FirstOrDefault(d => d.Id == result.Data.Id);
        saved.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_Should_Upload_Photo_And_Attach_Url_When_PhotoService_Enabled()
    {
        using var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var account = Account.Create("Donor With Photo", "photo@donor.com", "010", TestPasswordHelper.Hash(), UserRole.Donor);
        dbContext.Accounts.Add(account);
        await dbContext.SaveChangesAsync();

        var formFileMock = new Mock<IFormFile>();
        _photoServiceMock.Setup(p => p.IsEnabled).Returns(true);
        _photoServiceMock
            .Setup(p => p.UploadImageAsync(It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("https://cloudinary.com/photos/food.jpg");

        var handler = new CreateDonationCommandHandler(dbContext, _photoServiceMock.Object);
        var result = await handler.Handle(
            new CreateDonationCommand(account.Id, CreateDonationDto(formFileMock.Object)),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.ImageUrl.Should().Be("https://cloudinary.com/photos/food.jpg");
    }

    [Fact]
    public async Task Handle_Should_Throw_When_PhotoUpload_Returns_EmptyUrl()
    {
        using var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var account = Account.Create("Donor", "photo_fail@donor.com", "010", TestPasswordHelper.Hash(), UserRole.Donor);
        dbContext.Accounts.Add(account);
        await dbContext.SaveChangesAsync();

        var formFileMock = new Mock<IFormFile>();
        _photoServiceMock.Setup(p => p.IsEnabled).Returns(true);
        _photoServiceMock
            .Setup(p => p.UploadImageAsync(It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(string.Empty);

        var handler = new CreateDonationCommandHandler(dbContext, _photoServiceMock.Object);
        var action = () => handler.Handle(
            new CreateDonationCommand(account.Id, CreateDonationDto(formFileMock.Object)),
            CancellationToken.None);

        var exception = await action.Should().ThrowAsync<DomainException.FailureException>();
        exception.Which.Code.Should().Be(AppMessageCodes.IMAGE_UPLOAD_FAILED);
    }

    private static CreateDonationDTO CreateDonationDto(IFormFile? image = null)
    {
        return new CreateDonationDTO
        {
            Title = "Fresh Bread",
            Description = "Fresh bakery bread",
            FoodType = "Bakery",
            Quantity = "20 loaves",
            Weight = 4.5m,
            ExpiryDate = DateTime.UtcNow.AddDays(1),
            PickupLocation = "Bakery 1, Nasr City",
            CityId = 1,
            CategoryId = 3,
            Image = image
        };
    }
}
