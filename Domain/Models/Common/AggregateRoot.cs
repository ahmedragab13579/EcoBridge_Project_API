namespace EcoBridge.Domain.Models.Common;

/// <summary>
/// Abstract Base Class for all Aggregate Roots in DDD architecture.
/// </summary>
public abstract class AggregateRoot
{
    public Guid Id { get; protected set; }
}