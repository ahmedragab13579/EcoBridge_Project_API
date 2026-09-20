using System;
using System.Threading;
using System.Threading.Tasks;
using EcoBridge.Application.Contracts;
using EcoBridge.Application.DTO;
using EcoBridge.Application.Features.Auth.Commands;
using EcoBridge.Application.UnitTests.Helpers;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Errors;
using EcoBridge.Domain.Models.Accounts;
using FluentAssertions;
using Moq;
using Xunit;

namespace EcoBridge.Application.UnitTests.Commands;

public class UpdateUserProfileCommandHandlerTests
{
    private readonly Mock<ICacheService> _cacheServiceMock = new();
    private readonly Mock<ICloudinaryService> _cloudinaryServiceMock = new();

    [Fact]
    public async Task Handle_Should_Return_NotFound_When_Account_Does_Not_Exist()
    {
        // Arrange
        using var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var handler = new UpdateUserProfileCommandHandler(
            dbContext,
            _cacheServiceMock.Object,
            _cloudinaryServiceMock.Object);

        var command = new UpdateUserProfileCommand(Guid.NewGuid(), new UpdateProfileDTO
        {
            FullName = "Name",
            Phone = "01000000000"
        });

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.NotFound);
        result.Message.Should().Be(AppMessageCodes.USER_NOT_FOUND);
    }

    [Fact]
    public async Task Handle_Should_Update_Profile_And_Invalidate_Cache()
    {
        // Arrange
        using var dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        var account = Account.Create("Old Name", "user@test.com", "01011111111", TestPasswordHelper.Hash(), UserRole.Volunteer);
        var volunteer = Volunteer.Create(account.Id, "Bicycle", "Details");
        account.Volunteer = volunteer;

        dbContext.Accounts.Add(account);
        dbContext.Volunteers.Add(volunteer);
        await dbContext.SaveChangesAsync();

        var handler = new UpdateUserProfileCommandHandler(
            dbContext,
            _cacheServiceMock.Object,
            _cloudinaryServiceMock.Object);

        var command = new UpdateUserProfileCommand(account.Id, new UpdateProfileDTO
        {
            FullName = "New Name",
            Phone = "01022222222",
            CityId = 3,
            VehicleType = "Motorcycle"
        });

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Be(AppMessageCodes.PROFILE_UPDATED);
        result.Value.FullName.Should().Be("New Name");
        result.Value.Phone.Should().Be("01022222222");
        result.Value.CityId.Should().Be(3);
        result.Value.VehicleType.Should().Be("Motorcycle");

        _cacheServiceMock.Verify(c => c.RemoveAsync($"user_profile_{account.Id}", It.IsAny<CancellationToken>()), Times.Once);
        _cacheServiceMock.Verify(c => c.RemoveAsync($"user:{account.Id}", It.IsAny<CancellationToken>()), Times.Once);
    }
}
