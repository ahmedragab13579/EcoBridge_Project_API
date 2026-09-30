using EcoBridge.Application.Contracts;
using EcoBridge.Domain.Models;
using Newtonsoft.Json;

namespace EcoBridge.Infrastructure.Services.Implementation;

public class OutboxService(IEcoBridgeDbContext dbContext) : IOutboxService
{
    public void Enqueue<TEvent>(TEvent @event) where TEvent : class
    {
        var eventType = @event.GetType().AssemblyQualifiedName ?? @event.GetType().FullName ?? typeof(TEvent).FullName!;
        var content = JsonConvert.SerializeObject(@event);

        var outboxMessage = OutboxMessage.Create(eventType, content, DateTime.UtcNow);
        dbContext.OutboxMessages.Add(outboxMessage);
    }

    public async Task EnqueueAndSaveAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : class
    {
        Enqueue(@event);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
