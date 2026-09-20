using System;
using EcoBridge.Domain.Models.Accounts;
using FluentAssertions;
using Xunit;

namespace EcoBridge.Domain.UnitTests;

public class VolunteerTests
{
    [Fact]
    public void Create_Should_Initialize_With_Available_Status()
    {
        // Act
        var account = Account.Create("Test Volunteer", "vol@test.com", "01000", "hash", EcoBridge.Domain.Enums.UserRole.Volunteer, vehicleType: "Motorcycle", vehicleDetails: "Honda 2022 - Red");

        // Assert
        account.VehicleType.Should().Be("Motorcycle");
        account.VehicleDetails.Should().Be("Honda 2022 - Red");
        account.IsAvailable.Should().BeTrue();
    }

    [Fact]
    public void UpdateVehicle_Should_Update_Type_And_Details()
    {
        // Arrange
        var account = Account.Create("Test Volunteer", "vol@test.com", "01000", "hash", EcoBridge.Domain.Enums.UserRole.Volunteer, vehicleType: "Bicycle", vehicleDetails: "Standard");

        // Act
        account.UpdateVehicle("Van", "Toyota HiAce 2024");

        // Assert
        account.VehicleType.Should().Be("Van");
        account.VehicleDetails.Should().Be("Toyota HiAce 2024");
    }

    [Fact]
    public void SetAvailability_Should_Toggle_Availability()
    {
        // Arrange
        var account = Account.Create("Test Volunteer", "vol@test.com", "01000", "hash", EcoBridge.Domain.Enums.UserRole.Volunteer);

        // Act & Assert
        account.SetAvailability(false);
        account.IsAvailable.Should().BeFalse();

        account.SetAvailability(true);
        account.IsAvailable.Should().BeTrue();
    }
}
