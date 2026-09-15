using System;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Models;
using FluentAssertions;
using Xunit;

namespace EcoBridge.Domain.UnitTests;

public class DonationTests
{
    [Fact]
    public void Create_Should_Initialize_With_Correct_Default_Values()
    {
        // Arrange
        var donorId = Guid.NewGuid();
        var expiryDate = DateTime.UtcNow.AddDays(2);

        // Act
        var donation = Donation.Create(
            donorId: donorId,
            foodType: "Cooked Meals",
            quantity: "10 boxes",
            expiryDate: expiryDate,
            pickupLocation: "123 Main St, Cairo",
            categoryId: 1,
            title: "Surplus Catering Food",
            description: "Freshly prepared lunch boxes",
            weight: 5.5m,
            imageUrl: "https://example.com/food.jpg"
        );

        // Assert
        donation.Id.Should().NotBeEmpty();
        donation.DonorId.Should().Be(donorId);
        donation.FoodType.Should().Be("Cooked Meals");
        donation.Quantity.Should().Be("10 boxes");
        donation.ExpiryDate.Should().Be(expiryDate);
        donation.PickupLocation.Should().Be("123 Main St, Cairo");
        donation.CategoryId.Should().Be(1);
        donation.Title.Should().Be("Surplus Catering Food");
        donation.Description.Should().Be("Freshly prepared lunch boxes");
        donation.Weight.Should().Be(5.5m);
        donation.ImageUrl.Should().Be("https://example.com/food.jpg");
        donation.Status.Should().Be(DonationStatus.Available);
        donation.IsDeleted.Should().BeFalse();
        donation.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void UpdateDetails_Should_Update_Fields_Correctly()
    {
        // Arrange
        var donation = Donation.Create(
            Guid.NewGuid(), "Bread", "5 bags", DateTime.UtcNow.AddDays(1), "Old Location"
        );
        var newExpiry = DateTime.UtcNow.AddDays(3);

        // Act
        donation.UpdateDetails(
            title: "Updated Bread",
            description: "Fresh artisan loaves",
            foodType: "Bakery",
            quantity: "15 bags",
            weight: 3.0m,
            expiryDate: newExpiry,
            pickupLocation: "New Bakery Shop",
            categoryId: 2,
            imageUrl: "https://example.com/new.jpg"
        );

        // Assert
        donation.Title.Should().Be("Updated Bread");
        donation.Description.Should().Be("Fresh artisan loaves");
        donation.FoodType.Should().Be("Bakery");
        donation.Quantity.Should().Be("15 bags");
        donation.Weight.Should().Be(3.0m);
        donation.ExpiryDate.Should().Be(newExpiry);
        donation.PickupLocation.Should().Be("New Bakery Shop");
        donation.CategoryId.Should().Be(2);
        donation.ImageUrl.Should().Be("https://example.com/new.jpg");
    }

    [Fact]
    public void MarkAsRequested_Should_Set_Status_To_Pending()
    {
        // Arrange
        var donation = Donation.Create(Guid.NewGuid(), "Food", "10", DateTime.UtcNow.AddDays(1), "Loc");

        // Act
        donation.MarkAsRequested();

        // Assert
        donation.Status.Should().Be(DonationStatus.Pending);
    }

    [Fact]
    public void ReleaseCharityRequest_Should_Clear_CharityId_And_Set_Status_To_Available()
    {
        // Arrange
        var donation = Donation.Create(Guid.NewGuid(), "Food", "10", DateTime.UtcNow.AddDays(1), "Loc");
        var charityId = Guid.NewGuid();
        donation.MarkAsAccepted(charityId);

        // Act
        donation.ReleaseCharityRequest();

        // Assert
        donation.CharityId.Should().BeNull();
        donation.Status.Should().Be(DonationStatus.Available);
    }

    [Fact]
    public void MarkAsAccepted_Should_Set_CharityId_And_Status_To_Accepted()
    {
        // Arrange
        var donation = Donation.Create(Guid.NewGuid(), "Food", "10", DateTime.UtcNow.AddDays(1), "Loc");
        var charityId = Guid.NewGuid();

        // Act
        donation.MarkAsAccepted(charityId);

        // Assert
        donation.CharityId.Should().Be(charityId);
        donation.Status.Should().Be(DonationStatus.Accepted);
    }

    [Fact]
    public void AssignVolunteer_Should_Set_VolunteerId_And_Status_To_InTransit()
    {
        // Arrange
        var donation = Donation.Create(Guid.NewGuid(), "Food", "10", DateTime.UtcNow.AddDays(1), "Loc");
        var volunteerId = Guid.NewGuid();

        // Act
        donation.AssignVolunteer(volunteerId);

        // Assert
        donation.VolunteerId.Should().Be(volunteerId);
        donation.Status.Should().Be(DonationStatus.InTransit);
    }

    [Fact]
    public void MarkInTransit_Should_Set_Status_To_InTransit()
    {
        // Arrange
        var donation = Donation.Create(Guid.NewGuid(), "Food", "10", DateTime.UtcNow.AddDays(1), "Loc");

        // Act
        donation.MarkInTransit();

        // Assert
        donation.Status.Should().Be(DonationStatus.InTransit);
    }

    [Fact]
    public void MarkAsDelivered_Should_Set_Status_To_Delivered()
    {
        // Arrange
        var donation = Donation.Create(Guid.NewGuid(), "Food", "10", DateTime.UtcNow.AddDays(1), "Loc");

        // Act
        donation.MarkAsDelivered();

        // Assert
        donation.Status.Should().Be(DonationStatus.Delivered);
    }

    [Fact]
    public void Cancel_Should_Set_Status_To_Cancelled_And_Reason()
    {
        // Arrange
        var donation = Donation.Create(Guid.NewGuid(), "Food", "10", DateTime.UtcNow.AddDays(1), "Loc");

        // Act
        donation.Cancel("Item spoiled prior to pickup");

        // Assert
        donation.Status.Should().Be(DonationStatus.Cancelled);
        donation.CancellationReason.Should().Be("Item spoiled prior to pickup");
    }

    [Fact]
    public void SoftDelete_Should_Set_IsDeleted_To_True()
    {
        // Arrange
        var donation = Donation.Create(Guid.NewGuid(), "Food", "10", DateTime.UtcNow.AddDays(1), "Loc");

        // Act
        donation.SoftDelete();

        // Assert
        donation.IsDeleted.Should().BeTrue();
    }
}
