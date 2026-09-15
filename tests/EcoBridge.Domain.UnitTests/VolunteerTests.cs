using System;
using EcoBridge.Domain.Models;
using FluentAssertions;
using Xunit;

namespace EcoBridge.Domain.UnitTests;

public class VolunteerTests
{
    [Fact]
    public void Create_Should_Initialize_With_Available_Status()
    {
        // Arrange
        var accountId = Guid.NewGuid();

        // Act
        var volunteer = Volunteer.Create(accountId, "Motorcycle", "Honda 2022 - Red");

        // Assert
        volunteer.AccountId.Should().Be(accountId);
        volunteer.VehicleType.Should().Be("Motorcycle");
        volunteer.VehicleDetails.Should().Be("Honda 2022 - Red");
        volunteer.IsAvailable.Should().BeTrue();
    }

    [Fact]
    public void UpdateVehicle_Should_Update_Type_And_Details()
    {
        // Arrange
        var volunteer = Volunteer.Create(Guid.NewGuid(), "Bicycle", "Standard");

        // Act
        volunteer.UpdateVehicle("Van", "Toyota HiAce 2024");

        // Assert
        volunteer.VehicleType.Should().Be("Van");
        volunteer.VehicleDetails.Should().Be("Toyota HiAce 2024");
    }

    [Fact]
    public void SetAvailability_Should_Toggle_Availability()
    {
        // Arrange
        var volunteer = Volunteer.Create(Guid.NewGuid());

        // Act & Assert
        volunteer.SetAvailability(false);
        volunteer.IsAvailable.Should().BeFalse();

        volunteer.SetAvailability(true);
        volunteer.IsAvailable.Should().BeTrue();
    }
}
