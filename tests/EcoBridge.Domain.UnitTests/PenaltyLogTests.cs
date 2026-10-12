using System;
using EcoBridge.Domain.Models;
using FluentAssertions;
using Xunit;

namespace EcoBridge.Domain.UnitTests;

public class PenaltyLogTests
{
    [Fact]
    public void PenaltyLog_Should_Track_Account_And_Reason_Correctly()
    {
        // Arrange & Act
        var accountId = Guid.NewGuid();
        var missionId = Guid.NewGuid();
        var penalty = PenaltyLog.Create(
            accountId,
            15,
            "TIMEOUT_MISSED_PICKUP",
            missionId);

        // Assert
        penalty.AccountId.Should().Be(accountId);
        penalty.Amount.Should().Be(15);
        penalty.ReasonCode.Should().Be("TIMEOUT_MISSED_PICKUP");
        penalty.MissionId.Should().Be(missionId);
        penalty.CreatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }
}
