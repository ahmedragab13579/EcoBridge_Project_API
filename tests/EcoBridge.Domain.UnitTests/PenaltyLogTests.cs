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
        var penalty = new PenaltyLog
        {
            AccountId = accountId,
            Amount = 15,
            ReasonCode = "TIMEOUT_MISSED_PICKUP",
            MissionId = missionId,
            CreatedAtUtc = DateTime.UtcNow
        };

        // Assert
        penalty.AccountId.Should().Be(accountId);
        penalty.Amount.Should().Be(15);
        penalty.ReasonCode.Should().Be("TIMEOUT_MISSED_PICKUP");
        penalty.MissionId.Should().Be(missionId);
        penalty.CreatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }
}
