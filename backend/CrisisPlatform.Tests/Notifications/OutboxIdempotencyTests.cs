using CrisisPlatform.Api.Modules.Notifications;
using CrisisPlatform.Tests.Security;

namespace CrisisPlatform.Tests.Notifications;

/// <summary>Tests verifying <see cref="OutboxProcessor"/> suppresses duplicate delivery via idempotency keys.</summary>
public sealed class OutboxIdempotencyTests
{
    [Fact]
    public async Task ProcessBatchAsync_SuppressesDuplicateDelivery_ForSameIdempotencyKey()
    {
        var now   = new DateTimeOffset(2026, 6, 1, 10, 0, 0, TimeSpan.Zero);
        var clock = new ManualTimeProvider(now);
        var sut   = new OutboxProcessor(clock);

        int publishCalls = 0;

        var first = new OutboxMessage
        {
            IdempotencyKey = "req-notify-001",
            EventType      = "RequestReceived",
            NextAttemptAt  = now,
        };
        var duplicate = new OutboxMessage
        {
            IdempotencyKey = "req-notify-001",
            EventType      = "RequestReceived",
            NextAttemptAt  = now,
        };

        int delivered = await sut.ProcessBatchAsync([first, duplicate], (_, _) =>
        {
            publishCalls++;
            return Task.FromResult(true);
        });

        Assert.Equal(1, publishCalls);
        Assert.Equal(1, delivered);
        Assert.Equal(OutboxStatus.Delivered, first.Status);
        Assert.Equal(OutboxStatus.Delivered, duplicate.Status);
    }

    [Fact]
    public async Task ProcessBatchAsync_SuppressesDuplicateAcrossBatches()
    {
        var now   = new DateTimeOffset(2026, 6, 1, 10, 0, 0, TimeSpan.Zero);
        var clock = new ManualTimeProvider(now);
        var sut   = new OutboxProcessor(clock);

        int publishCalls = 0;
        Task<bool> Publisher(OutboxMessage _, CancellationToken __)
        {
            publishCalls++;
            return Task.FromResult(true);
        }

        var msg1 = new OutboxMessage { IdempotencyKey = "evt-xyz", NextAttemptAt = now };
        await sut.ProcessBatchAsync([msg1], Publisher);

        var msg2 = new OutboxMessage { IdempotencyKey = "evt-xyz", NextAttemptAt = now };
        await sut.ProcessBatchAsync([msg2], Publisher);

        Assert.Equal(1, publishCalls);
        Assert.Equal(OutboxStatus.Delivered, msg2.Status);
    }
}
