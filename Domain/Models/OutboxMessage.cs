namespace EcoBridge.Domain.Models;

public class OutboxMessage
{
    public Guid Id { get; private set; }
    public string EventType { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public DateTime OccurredOnUtc { get; private set; }
    public DateTime? ProcessedOnUtc { get; private set; }
    public string? Error { get; private set; }
    public int RetryCount { get; private set; }
    public DateTime? NextRetryOnUtc { get; private set; }

    private OutboxMessage() { } // Required for EF Core

    public static OutboxMessage Create(string eventType, string content, DateTime? occurredOnUtc = null)
    {
        return new OutboxMessage
        {
            Id = Guid.NewGuid(),
            EventType = eventType,
            Content = content,
            OccurredOnUtc = occurredOnUtc ?? DateTime.UtcNow,
            ProcessedOnUtc = null,
            Error = null,
            RetryCount = 0,
            NextRetryOnUtc = null
        };
    }

    public void MarkProcessed(DateTime? processedOnUtc = null)
    {
        ProcessedOnUtc = processedOnUtc ?? DateTime.UtcNow;
        Error = null;
        NextRetryOnUtc = null;
    }

    public void MarkFailed(string error, int maxRetries = 5, int baseDelaySeconds = 10)
    {
        Error = error;
        RetryCount++;

        if (RetryCount <= maxRetries)
        {
            var delaySeconds = Math.Pow(2, Math.Min(RetryCount, 6)) * baseDelaySeconds;
            NextRetryOnUtc = DateTime.UtcNow.AddSeconds(delaySeconds);
        }
        else
        {
            // Max retries exceeded (dead letter)
            NextRetryOnUtc = null;
        }
    }
}
