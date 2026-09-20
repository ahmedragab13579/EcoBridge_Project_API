using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EcoBridge.Application.Contracts;
using EcoBridge.Application.DTO;
using EcoBridge.Application.Features.Donation.Commands;
using EcoBridge.Application.UnitTests.Helpers;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Models.Accounts;
using EcoBridge.Domain.Results;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace EcoBridge.Application.UnitTests.Commands;

public class CreateDonationCommandHandlerTests
{
    private readonly Mock<IPhotoService> _photoServiceMock = new();

    [Fact]
    public async Task Handle_Should_Return_NotFound_When_Donor_Does_Not_Exist()
    {
        // Arrange
        using var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var handler = new CreateDonationCommandHandler(dbContext, _photoServiceMock.Object);

        var command = new CreateDonationCommand(Guid.NewGuid(), new CreateDonationDTO
        {
            FoodType = "Vegetables",
            Quantity = "5 kg",
            ExpiryDate = DateTime.UtcNow.AddDays(2),
            PickupLocation = "Downtown"
        });

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.NotFound);
        result.Message.Should().Be(AppMessageCodes.DONOR_NOT_FOUND);
    }

    [Fact]
    public async Task Handle_Should_Create_Donation_Successfully_Without_Image()
    {
        // Arrange
        using var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var account = Account.Create("Donor Name", "donor@test.com", "010", TestPasswordHelper.Hash(), UserRole.Donor);

        dbContext.Accounts.Add(account);
        await dbContext.SaveChangesAsync();

        var handler = new CreateDonationCommandHandler(dbContext, _photoServiceMock.Object);

        var command = new CreateDonationCommand(account.Id, new CreateDonationDTO
        {
            Title = "Fresh Bread",
            FoodType = "Bakery",
            Quantity = "20 loaves",
            Weight = 4.5m,
            ExpiryDate = DateTime.UtcNow.AddDays(1),
            PickupLocation = "Bakery 1, Nasr City",
            CategoryId = 3
        });

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Be(AppMessageCodes.DONATION_CREATED);
        result.Value.Title.Should().Be("Fresh Bread");
        result.Value.FoodType.Should().Be("Bakery");
        result.Value.Quantity.Should().Be("20 loaves");
        result.Value.Status.Should().Be(DonationStatus.Available.ToString());
        result.Value.DonorId.Should().Be(account.Id);

        var saved = dbContext.Donations.FirstOrDefault(d => d.Id == result.Value.Id);
        saved.Should().NotBeNull();
        saved!.Title.Should().Be("Fresh Bread");
    }

    [Fact]
    public async Task Handle_Should_Upload_Photo_And_Attach_Url_When_PhotoService_Enabled()
    {
        // Arrange
        using var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var account = Account.Create("Donor With Photo", "photo@donor.com", "010", TestPasswordHelper.Hash(), UserRole.Donor);

        dbContext.Accounts.Add(account);
        await dbContext.SaveChangesAsync();

        var formFileMock = new Mock<IFormFile>();
        _photoServiceMock.Setup(p => p.IsEnabled).Returns(true);
        _photoServiceMock
            .Setup(p => p.UploadImageAsync(It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.SuccessResult("https://cloudinary.com/photos/food.jpg"));

        var handler = new CreateDonationCommandHandler(dbContext, _photoServiceMock.Object);

        var command = new CreateDonationCommand(account.Id, new CreateDonationDTO
        {
            Title = "Cooked Rice",
            Quantity = "10 boxes",
            ExpiryDate = DateTime.UtcNow.AddDays(1),
            PickupLocation = "Restaurant 5",
            Image = formFileMock.Object
        });

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.ImageUrl.Should().Be("https://cloudinary.com/photos/food.jpg");
    }

    [Fact]
    public async Task Handle_Should_Fail_When_PhotoUpload_Fails()
    {
        // Arrange
        using var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var account = Account.Create("Donor", "photo_fail@donor.com", "010", TestPasswordHelper.Hash(), UserRole.Donor);

        dbContext.Accounts.Add(account);
        await dbContext.SaveChangesAsync();

        var formFileMock = new Mock<IFormFile>();
        _photoServiceMock.Setup(p => p.IsEnabled).Returns(true);
        _photoServiceMock
            .Setup(p => p.UploadImageAsync(It.IsAny<IFormFile>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.FailResult(string.Empty, "Invalid image format", ErrorType.Validation));

        var handler = new CreateDonationCommandHandler(dbContext, _photoServiceMock.Object);

        var command = new CreateDonationCommand(account.Id, new CreateDonationDTO
        {
            Title = "Cooked Rice",
            Quantity = "10 boxes",
            ExpiryDate = DateTime.UtcNow.AddDays(1),
            PickupLocation = "Restaurant 5",
            Image = formFileMock.Object
        });

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.Validation);
        result.Message.Should().Be("Invalid image format");
    }
}
