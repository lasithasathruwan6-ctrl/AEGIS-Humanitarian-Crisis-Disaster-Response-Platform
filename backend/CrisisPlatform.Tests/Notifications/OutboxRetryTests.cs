using CrisisPlatform.Api.Modules.Notifications;
using CrisisPlatform.Tests.Security;

namespace CrisisPlatform.Tests.Notifications;

/// <summary>Tests for <see cref="OutboxProcessor"/> exponential-backoff retry and dead-lettering.</summary>
public sealed class OutboxRetryTests
{
    [Fact]
    public async Task ProcessBatchAsync_SchedulesExponentialBackoff_OnFailure()
    {
        var start = new DateTimeOffset(2026, 6, 1, 10, 0, 0, TimeSpan.Zero);
        var clock = new ManualTimeProvider(start);
        var sut   = new OutboxProcessor(clock, maxAttempts: 4, baseDelaySeconds: 10);

        var msg = new OutboxMessage
        {
            IdempotencyKey = "evt-backoff-1",
            EventType      = "AidDispatched",
            NextAttemptAt  = start,
        };

        // Attempt 1 fails -> backoff 10s
        await sut.ProcessBatchAsync([msg], (_, _) => Task.FromResult(false));
        Assert.Equal(OutboxStatus.Pending, msg.Status);
        Assert.Equal(1, msg.AttemptCount);
        Assert.Equal(start.AddSeconds(10), msg.NextAttemptAt);

        // Before backoff elapses, it must NOT be retried
        clock.Advance(TimeSpan.FromSeconds(5));
        await sut.ProcessBatchAsync([msg], (_, _) => Task.FromResult(false));
        Assert.Equal(1, msg.AttemptCount);

        // Attempt 2 fails -> backoff 20s
        clock.Advance(TimeSpan.FromSeconds(5));
        await sut.ProcessBatchAsync([msg], (_, _) => Task.FromResult(false));
        Assert.Equal(2, msg.AttemptCount);
        Assert.Equal(start.AddSeconds(30), msg.NextAttemptAt);

        // Attempt 3 fails -> backoff 40s
        clock.Advance(TimeSpan.FromSeconds(20));
        await sut.ProcessBatchAsync([msg], (_, _) => Task.FromResult(false));
        Assert.Equal(3, msg.AttemptCount);
        Assert.Equal(start.AddSeconds(70), msg.NextAttemptAt);

        // Attempt 4 fails -> dead-lettered
        clock.Advance(TimeSpan.FromSeconds(40));
        await sut.ProcessBatchAsync([msg], (_, _) => Task.FromResult(false));
        Assert.Equal(OutboxStatus.DeadLettered, msg.Status);
        Assert.Equal(4, msg.AttemptCount);
    }
}
