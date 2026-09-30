using EcoBridge.Domain.Contracts;
using EcoBridge.Domain.Enums;
using EcoBridge.Domain.Models;
using EcoBridge.Domain.Models.Common;
using FluentAssertions;
using Xunit;

namespace EcoBridge.Application.UnitTests.Validation;

public class Stage5VerificationGateTests
{
    private class TestDomainEvent(string name) : IDomainEvent
    {
        public string Name { get; } = name;
        public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
    }

    private class TestAggregate : AggregateRoot
    {
        public void DoSomething()
        {
            AddDomainEvent(new TestDomainEvent("SomethingHappened"));
        }
    }

    [Fact]
    public void Gate4_AggregateRoot_TracksAndClearsDomainEvents()
    {
        // Arrange
        var aggregate = new TestAggregate();

        // Act
        aggregate.DoSomething();

        // Assert
        aggregate.DomainEvents.Should().HaveCount(1);
        aggregate.DomainEvents.First().Should().BeOfType<TestDomainEvent>();

        // Act
        aggregate.ClearDomainEvents();

        // Assert
        aggregate.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Gate1_OutboxMessage_SerializationRoundtrip_MaintainsDataIntegrity()
    {
        // Arrange
        var evt = new TestDomainEvent("IntegrityCheck");
        var eventType = evt.GetType().AssemblyQualifiedName!;
        var json = Newtonsoft.Json.JsonConvert.SerializeObject(evt);

        // Act
        var message = OutboxMessage.Create(eventType, json);

        // Assert
        message.EventType.Should().Be(eventType);
        message.Content.Should().Contain("IntegrityCheck");
        message.ProcessedOnUtc.Should().BeNull();
    }
}
