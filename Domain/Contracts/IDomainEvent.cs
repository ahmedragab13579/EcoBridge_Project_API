namespace EcoBridge.Domain.Contracts;

public interface IDomainEvent
{
    DateTime OccurredOnUtc { get; }
}
