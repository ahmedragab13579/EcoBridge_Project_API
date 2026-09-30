namespace EcoBridge.Application.Contracts;

public interface IOutboxService
{
    void Enqueue<TEvent>(TEvent @event) where TEvent : class;
    Task EnqueueAndSaveAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : class;
}
