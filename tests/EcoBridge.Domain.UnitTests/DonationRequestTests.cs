using System;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Models;
using FluentAssertions;
using Xunit;

namespace EcoBridge.Domain.UnitTests;

public class DonationRequestTests
{
    [Fact]
    public void Create_Should_Initialize_With_Pending_Status()
    {
        // Arrange
        var donationId = Guid.NewGuid();
        var charityId = Guid.NewGuid();

        // Act
        var request = DonationRequest.Create(donationId, charityId);

        // Assert
        request.Id.Should().NotBeEmpty();
        request.DonationId.Should().Be(donationId);
        request.CharityId.Should().Be(charityId);
        request.RequestStatus.Should().Be(RequestStatus.Pending);
        request.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Approve_Should_Set_Status_To_ApprovedByDonor()
    {
        // Arrange
        var request = DonationRequest.Create(Guid.NewGuid(), Guid.NewGuid());

        // Act
        request.Approve();

        // Assert
        request.RequestStatus.Should().Be(RequestStatus.ApprovedByDonor);
    }

    [Fact]
    public void Reject_Should_Set_Status_To_Rejected()
    {
        // Arrange
        var request = DonationRequest.Create(Guid.NewGuid(), Guid.NewGuid());

        // Act
        request.Reject();

        // Assert
        request.RequestStatus.Should().Be(RequestStatus.Rejected);
    }
}
