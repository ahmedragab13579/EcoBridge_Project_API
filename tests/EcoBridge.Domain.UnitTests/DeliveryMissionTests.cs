using System;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Models.Mission;
using FluentAssertions;
using Xunit;

namespace EcoBridge.Domain.UnitTests;

public class DeliveryMissionTests
{
    private static DeliveryMission CreateMission()
    {
        return DeliveryMission.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Donor Hub, Dokki",
            "Charity Center, Maadi");
    }

    [Fact]
    public void Create_Should_Initialize_With_Assigned_Status()
    {
        var donationId = Guid.NewGuid();
        var volunteerId = Guid.NewGuid();

        var mission = DeliveryMission.Create(
            donationId,
            volunteerId,
            "Donor Hub, Dokki",
            "Charity Center, Maadi");

        mission.Id.Should().NotBeEmpty();
        mission.DonationId.Should().Be(donationId);
        mission.VolunteerId.Should().Be(volunteerId);
        mission.Status.Should().Be(MissionStatus.Assigned);
        mission.PickupTime.Should().BeNull();
        mission.DeliveryTime.Should().BeNull();
    }

    [Fact]
    public void ConfirmPickup_Should_Set_InTransit_And_Clear_Timeout_Job()
    {
        var mission = CreateMission();
        mission.AttachTimeoutJob("job-abc");

        mission.ConfirmPickup();

        mission.Status.Should().Be(MissionStatus.InTransit);
        mission.PickupTime.Should().NotBeNull();
        mission.HangfireJobId.Should().BeNull();
    }

    [Fact]
    public void RecordDelivery_Should_Set_Delivered_And_DeliveryTime()
    {
        var mission = CreateMission();
        mission.ConfirmPickup();

        mission.RecordDelivery();

        mission.Status.Should().Be(MissionStatus.Delivered);
        mission.DeliveryTime.Should().NotBeNull();
    }

    [Fact]
    public void Cancel_Should_Set_Cancelled_Status_And_Reason()
    {
        var mission = CreateMission();

        mission.Cancel("Volunteer unavailable");

        mission.Status.Should().Be(MissionStatus.Cancelled);
        mission.CancellationReason.Should().Be("Volunteer unavailable");
    }

    [Fact]
    public void IssueOtp_AfterPickup_Should_Create_A_ValidOtp()
    {
        var mission = CreateMission();
        mission.ConfirmPickup();

        mission.OtpCode.Should().HaveLength(6);
        mission.CheckOtp(mission.OtpCode).Should().Be(OtpCheckResult.Valid);
    }
}
