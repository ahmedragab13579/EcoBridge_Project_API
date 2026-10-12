using System;
using EcoBridge.Domain.Models;
using FluentAssertions;
using Xunit;

namespace EcoBridge.Domain.UnitTests;

public class ReviewTests
{
    [Fact]
    public void Create_Should_Initialize_Review_Properties()
    {
        // Arrange
        var reviewerId = Guid.NewGuid();
        var revieweeId = Guid.NewGuid();
        var missionId = Guid.NewGuid();

        // Act
        var review = Review.Create(
            reviewerId: reviewerId,
            revieweeId: revieweeId,
            reating: 5,
            reviewReasonId: 1,
            missionId: missionId,
            comment: "Very punctual and friendly volunteer"
        );

        // Assert
        review.ReviewerId.Should().Be(reviewerId);
        review.RevieweeId.Should().Be(revieweeId);
        review.Rating.Should().Be(5);
        review.ReviewReasonId.Should().Be(1);
        review.Comment.Should().Be("Very punctual and friendly volunteer");
        review.MissionId.Should().Be(missionId);
        review.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }
}
