using System;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Models;
using FluentAssertions;
using Xunit;

namespace EcoBridge.Domain.UnitTests;

public class PointTransactionTests
{
    [Fact]
    public void Create_Should_Initialize_Properties_Correctly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var referenceId = Guid.NewGuid();

        // Act
        var transaction = PointTransaction.Create(
            userId: userId,
            amount: 100,
            transactionType: TransactionType.MissionCompleted,
            referenceId: referenceId
        );

        // Assert
        transaction.UserId.Should().Be(userId);
        transaction.Amount.Should().Be(100);
        transaction.TransactionType.Should().Be(TransactionType.MissionCompleted);
        transaction.ReferenceId.Should().Be(referenceId);
        transaction.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }
}
