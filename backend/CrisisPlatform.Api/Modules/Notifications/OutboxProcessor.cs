namespace CrisisPlatform.Api.Modules.Notifications;

public enum OutboxStatus
{
    Pending,
    Delivered,
    DeadLettered,
}

public sealed class OutboxMessage
{
    public Guid           Id             { get; init; } = Guid.NewGuid();
    public string         IdempotencyKey { get; init; } = string.Empty;
    public string         EventType      { get; init; } = string.Empty;
    public string         PayloadJson    { get; init; } = string.Empty;
    public OutboxStatus   Status         { get; set; }  = OutboxStatus.Pending;
    public int            AttemptCount   { get; set; }
    public DateTimeOffset NextAttemptAt  { get; set; }
    public string?        LastError      { get; set; }
}

/// <summary>
/// Processes pending <see cref="OutboxMessage"/> records with exponential backoff
/// and idempotency-key deduplication.
/// </summary>
public sealed class OutboxProcessor(
    TimeProvider clock,
    int maxAttempts = 5,
    int baseDelaySeconds = 10)
{
    private readonly HashSet<string> _deliveredKeys = new(StringComparer.Ordinal);

    /// <summary>Computes exponential backoff delay: <c>baseDelay * 2^(attempt - 1)</c>.</summary>
    public TimeSpan ComputeBackoff(int attempt) =>
        TimeSpan.FromSeconds(baseDelaySeconds * Math.Pow(2, Math.Max(0, attempt - 1)));

    public async Task<int> ProcessBatchAsync(
        IList<OutboxMessage> messages,
        Func<OutboxMessage, CancellationToken, Task<bool>> publishAsync,
        CancellationToken cancellationToken = default)
    {
        var now = clock.GetUtcNow();
        int delivered = 0;

        foreach (var msg in messages.Where(m => m.Status == OutboxStatus.Pending && m.NextAttemptAt <= now))
        {
            if (!string.IsNullOrWhiteSpace(msg.IdempotencyKey) && _deliveredKeys.Contains(msg.IdempotencyKey))
            {
                msg.Status = OutboxStatus.Delivered;
                continue;
            }

            msg.AttemptCount++;
            bool ok;
            try
            {
                ok = await publishAsync(msg, cancellationToken);
            }
            catch (Exception ex)
            {
                ok = false;
                msg.LastError = ex.Message;
            }

            if (ok)
            {
                msg.Status = OutboxStatus.Delivered;
                if (!string.IsNullOrWhiteSpace(msg.IdempotencyKey))
                    _deliveredKeys.Add(msg.IdempotencyKey);
                delivered++;
            }
            else if (msg.AttemptCount >= maxAttempts)
            {
                msg.Status = OutboxStatus.DeadLettered;
                msg.LastError ??= "Max retry attempts reached.";
            }
            else
            {
                msg.NextAttemptAt = now + ComputeBackoff(msg.AttemptCount);
            }
        }

        return delivered;
    }
}
