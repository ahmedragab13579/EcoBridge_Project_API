using EcoBridge.Domain.Contracts;

namespace EcoBridge.Domain.Models.Common;

/// <summary>
/// Abstract Base Class for all Aggregate Roots in DDD architecture.
/// </summary>
public abstract class AggregateRoot
{
    public Guid Id { get; protected set; }

    private readonly List<IDomainEvent> _domainEvents = new();

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}