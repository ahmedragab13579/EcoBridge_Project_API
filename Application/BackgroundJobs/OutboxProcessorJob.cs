using EcoBridge.Application.Contracts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Reflection;

namespace EcoBridge.Application.BackgroundJobs;

public class OutboxProcessorJob(
    IEcoBridgeDbContext dbContext,
    IPublisher publisher,
    ILogger<OutboxProcessorJob> logger
)
{
    private static readonly ConcurrentDictionary<string, Type?> TypeCache = new();

    public async Task ProcessPendingMessagesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        var messages = await dbContext.OutboxMessages
            .Where(m => m.ProcessedOnUtc == null && (m.NextRetryOnUtc == null || m.NextRetryOnUtc <= now))
            .OrderBy(m => m.OccurredOnUtc)
            .Take(50)
            .ToListAsync(cancellationToken);

        if (messages.Count == 0)
            return;

        foreach (var message in messages)
        {
            try
            {
                var eventType = ResolveType(message.EventType);
                if (eventType is null)
                {
                    logger.LogError("Could not resolve event type '{EventType}' for OutboxMessage {MessageId}", message.EventType, message.Id);
                    message.MarkFailed($"Could not resolve type: {message.EventType}");
                    continue;
                }

                var eventObj = JsonConvert.DeserializeObject(message.Content, eventType);
                if (eventObj is null)
                {
                    logger.LogError("Failed to deserialize content for OutboxMessage {MessageId} into {EventType}", message.Id, message.EventType);
                    message.MarkFailed("Deserialization produced null object.");
                    continue;
                }

                if (eventObj is INotification notification)
                {
                    await publisher.Publish(notification, cancellationToken);
                }
                else
                {
                    logger.LogWarning("Outbox message {MessageId} payload type {EventType} does not implement INotification.", message.Id, message.EventType);
                }

                message.MarkProcessed();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing OutboxMessage {MessageId} of type {EventType}", message.Id, message.EventType);
                message.MarkFailed(ex.ToString());
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static Type? ResolveType(string typeName)
    {
        return TypeCache.GetOrAdd(typeName, name =>
        {
            var type = Type.GetType(name);
            if (type is not null)
                return type;

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = assembly.GetType(name);
                if (type is not null)
                    return type;
            }

            return null;
        });
    }
}
