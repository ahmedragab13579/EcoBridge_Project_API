using System;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Models.Mission;
using FluentAssertions;
using Xunit;

namespace EcoBridge.Domain.UnitTests;

public class DeliveryMissionTests
{
    [Fact]
    public void Create_Should_Initialize_With_SearchingForVolunteer_Status()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var otpExpires = DateTime.UtcNow.AddHours(24);

        // Act
        var mission = DeliveryMission.Create(
            donationRequestId: requestId,
            pickupLocation: "Donor Hub, Dokki",
            dropOffLocation: "Charity Center, Maadi",
            otpCode: "4321",
            otpExpiresAtUtc: otpExpires
        );

        // Assert
        mission.Id.Should().NotBeEmpty();
        mission.DonationRequestId.Should().Be(requestId);
        mission.PickupLocation.Should().Be("Donor Hub, Dokki");
        mission.DropOffLocation.Should().Be("Charity Center, Maadi");
        mission.OtpCode.Should().Be("4321");
        mission.OtpExpiresAtUtc.Should().Be(otpExpires);
        mission.MissionStatus.Should().Be(MissionStatus.SearchingForVolunteer);
        mission.VolunteerId.Should().BeNull();
        mission.PickupTime.Should().BeNull();
        mission.DeliveryTime.Should().BeNull();
    }

    [Fact]
    public void Accept_Should_Assign_VolunteerId_And_JobId_And_Set_Status_To_Accepted()
    {
        // Arrange
        var mission = DeliveryMission.Create(Guid.NewGuid(), "Pickup", "DropOff");
        var volunteerId = Guid.NewGuid();
        var jobId = "hangfire-job-123";

        // Act
        mission.Accept(volunteerId, jobId);

        // Assert
        mission.VolunteerId.Should().Be(volunteerId);
        mission.MissionStatus.Should().Be(MissionStatus.Accepted);
        mission.HangfireJobId.Should().Be("hangfire-job-123");
    }

    [Fact]
    public void ConfirmPickup_Should_Set_InTransit_And_PickupTime_And_Clear_HangfireJobId()
    {
        // Arrange
        var mission = DeliveryMission.Create(Guid.NewGuid(), "Pickup", "DropOff");
        mission.Accept(Guid.NewGuid(), "job-abc");

        // Act
        mission.ConfirmPickup();

        // Assert
        mission.MissionStatus.Should().Be(MissionStatus.InTransit);
        mission.PickupTime.Should().NotBeNull();
        mission.PickupTime.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        mission.HangfireJobId.Should().BeNull();
    }

    [Fact]
    public void Deliver_Should_Set_Delivered_And_DeliveryTime()
    {
        // Arrange
        var mission = DeliveryMission.Create(Guid.NewGuid(), "Pickup", "DropOff");
        mission.Accept(Guid.NewGuid());
        mission.ConfirmPickup();

        // Act
        mission.Deliver();

        // Assert
        mission.MissionStatus.Should().Be(MissionStatus.Delivered);
        mission.DeliveryTime.Should().NotBeNull();
        mission.DeliveryTime.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Cancel_Should_Set_Cancelled_Status()
    {
        // Arrange
        var mission = DeliveryMission.Create(Guid.NewGuid(), "Pickup", "DropOff");

        // Act
        mission.Cancel();

        // Assert
        mission.MissionStatus.Should().Be(MissionStatus.Cancelled);
    }

    [Fact]
    public void SetOtp_Should_Set_OtpCode_And_Expiry()
    {
        // Arrange
        var mission = DeliveryMission.Create(Guid.NewGuid(), "Pickup", "DropOff");
        var expiry = DateTime.UtcNow.AddMinutes(30);

        // Act
        mission.SetOtp("8899", expiry);

        // Assert
        mission.OtpCode.Should().Be("8899");
        mission.OtpExpiresAtUtc.Should().Be(expiry);
    }
}
