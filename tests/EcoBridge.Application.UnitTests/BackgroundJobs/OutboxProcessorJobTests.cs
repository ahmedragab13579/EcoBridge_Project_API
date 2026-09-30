using EcoBridge.Application.BackgroundJobs;
using EcoBridge.Application.Contracts;
using EcoBridge.Application.Features.Events;
using EcoBridge.Domain.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Xunit;
using FluentAssertions;

namespace EcoBridge.Application.UnitTests.BackgroundJobs;

public class OutboxProcessorJobTests
{
    private readonly Mock<IPublisher> _publisherMock;
    private readonly Mock<ILogger<OutboxProcessorJob>> _loggerMock;
    private readonly Mock<IEcoBridgeDbContext> _dbContextMock;
    private readonly Mock<DbSet<OutboxMessage>> _outboxDbSetMock;
    private readonly List<OutboxMessage> _outboxMessages;

    public OutboxProcessorJobTests()
    {
        _publisherMock = new Mock<IPublisher>();
        _loggerMock = new Mock<ILogger<OutboxProcessorJob>>();
        _dbContextMock = new Mock<IEcoBridgeDbContext>();
        _outboxDbSetMock = new Mock<DbSet<OutboxMessage>>();
        _outboxMessages = new List<OutboxMessage>();
    }

    [Fact]
    public void OutboxMessage_Create_InitializesPendingStateCorrectly()
    {
        // Arrange
        var evt = new DeliveryCompletedEvent(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        var eventType = evt.GetType().AssemblyQualifiedName!;
        var content = JsonConvert.SerializeObject(evt);

        // Act
        var outboxMessage = OutboxMessage.Create(eventType, content);

        // Assert
        outboxMessage.Id.Should().NotBeEmpty();
        outboxMessage.EventType.Should().Be(eventType);
        outboxMessage.Content.Should().Be(content);
        outboxMessage.ProcessedOnUtc.Should().BeNull();
        outboxMessage.Error.Should().BeNull();
        outboxMessage.RetryCount.Should().Be(0);
        outboxMessage.NextRetryOnUtc.Should().BeNull();
    }

    [Fact]
    public void OutboxMessage_MarkProcessed_SetsTimestampAndClearsErrors()
    {
        // Arrange
        var outboxMessage = OutboxMessage.Create("Type", "{}");
        outboxMessage.MarkFailed("Some transient error");

        // Act
        outboxMessage.MarkProcessed();

        // Assert
        outboxMessage.ProcessedOnUtc.Should().NotBeNull();
        outboxMessage.Error.Should().BeNull();
        outboxMessage.NextRetryOnUtc.Should().BeNull();
    }

    [Fact]
    public void OutboxMessage_MarkFailed_IncrementsRetryCountAndCalculatesBackoff()
    {
        // Arrange
        var outboxMessage = OutboxMessage.Create("Type", "{}");

        // Act - 1st failure
        outboxMessage.MarkFailed("Network timeout", maxRetries: 5, baseDelaySeconds: 10);

        // Assert
        outboxMessage.RetryCount.Should().Be(1);
        outboxMessage.Error.Should().Be("Network timeout");
        outboxMessage.NextRetryOnUtc.Should().NotBeNull();
        outboxMessage.NextRetryOnUtc.Should().BeAfter(DateTime.UtcNow.AddSeconds(5));

        // Act - Exceed max retries (e.g. 5 more times)
        for (int i = 0; i < 5; i++)
        {
            outboxMessage.MarkFailed("Repeated error", maxRetries: 5, baseDelaySeconds: 10);
        }

        // Assert - dead letter
        outboxMessage.RetryCount.Should().Be(6);
        outboxMessage.NextRetryOnUtc.Should().BeNull();
    }
}
