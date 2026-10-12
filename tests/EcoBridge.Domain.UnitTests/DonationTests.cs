using System;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Models.Donation;
using FluentAssertions;
using Xunit;

namespace EcoBridge.Domain.UnitTests;

public class DonationTests
{
    private static Donation CreateDonation(DateTime? expiryDate = null)
    {
        return Donation.Create(
            donorId: Guid.NewGuid(),
            foodType: "Cooked Meals",
            quantity: "10 boxes",
            expiryDate: expiryDate ?? DateTime.UtcNow.AddDays(2),
            pickupLocation: "123 Main St, Cairo",
            cityId: 1,
            title: "Surplus Catering Food",
            description: "Freshly prepared lunch boxes",
            categoryId: 1,
            weight: 5.5m,
            imageUrl: "https://example.com/food.jpg");
    }

    [Fact]
    public void Create_Should_Initialize_With_Correct_Default_Values()
    {
        var donorId = Guid.NewGuid();
        var expiryDate = DateTime.UtcNow.AddDays(2);

        var donation = Donation.Create(
            donorId,
            "Cooked Meals",
            "10 boxes",
            expiryDate,
            "123 Main St, Cairo",
            1,
            "Surplus Catering Food",
            "Freshly prepared lunch boxes",
            1,
            5.5m,
            "https://example.com/food.jpg");

        donation.Id.Should().NotBeEmpty();
        donation.DonorId.Should().Be(donorId);
        donation.QuantityDescription.Should().Be("10 boxes");
        donation.WeightKg.Should().Be(5.5m);
        donation.CityId.Should().Be(1);
        donation.Status.Should().Be(DonationStatus.Available);
        donation.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void UpdateDetails_Should_Update_Fields_Correctly()
    {
        var donation = CreateDonation();
        var newExpiry = DateTime.UtcNow.AddDays(3);

        donation.UpdateDetails(
            title: "Updated Bread",
            description: "Fresh artisan loaves",
            foodType: "Bakery",
            quantity: "15 bags",
            weight: 3.0m,
            expiryDate: newExpiry,
            pickupLocation: "New Bakery Shop",
            categoryId: 2,
            cityId: 2,
            imageUrl: "https://example.com/new.jpg");

        donation.Title.Should().Be("Updated Bread");
        donation.Description.Should().Be("Fresh artisan loaves");
        donation.QuantityDescription.Should().Be("15 bags");
        donation.WeightKg.Should().Be(3.0m);
        donation.CityId.Should().Be(2);
        donation.ExpiryDate.Should().Be(newExpiry);
    }

    [Fact]
    public void Claim_Should_Set_Charity_And_Searching_Status()
    {
        var donation = CreateDonation();
        var charityId = Guid.NewGuid();

        donation.Claim(charityId);

        donation.CharityId.Should().Be(charityId);
        donation.Status.Should().Be(DonationStatus.SearchingForVolunteer);
    }

    [Fact]
    public void AssignVolunteer_And_Complete_Should_Advance_Lifecycle()
    {
        var donation = CreateDonation();
        var volunteerId = Guid.NewGuid();
        donation.Claim(Guid.NewGuid());

        donation.AssignVolunteer(volunteerId);
        donation.Complete();

        donation.VolunteerId.Should().Be(volunteerId);
        donation.Status.Should().Be(DonationStatus.Completed);
    }

    [Fact]
    public void Cancel_Should_Set_Status_And_Reason()
    {
        var donation = CreateDonation();

        donation.Cancel("Item spoiled prior to pickup");

        donation.Status.Should().Be(DonationStatus.Cancelled);
        donation.CancellationReason.Should().Be("Item spoiled prior to pickup");
    }

    [Fact]
    public void SoftDelete_Should_Set_IsDeleted_To_True()
    {
        var donation = CreateDonation();

        donation.SoftDelete();

        donation.IsDeleted.Should().BeTrue();
        donation.Status.Should().Be(DonationStatus.Cancelled);
    }
}
