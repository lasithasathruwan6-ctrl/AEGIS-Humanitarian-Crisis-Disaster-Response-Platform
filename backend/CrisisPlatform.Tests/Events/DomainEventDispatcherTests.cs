using CrisisPlatform.Api.Common.Events;
using Microsoft.Extensions.DependencyInjection;

namespace CrisisPlatform.Tests.Events;

/// <summary>Unit tests for <see cref="DomainEventDispatcher"/>.</summary>
public sealed class DomainEventDispatcherTests
{
    private sealed record AidRequestedEvent(Guid RequestId, DateTimeOffset OccurredAt) : IDomainEvent;
    private sealed record ShelterAssignedEvent(Guid ShelterId, DateTimeOffset OccurredAt) : IDomainEvent;

    private sealed class RecordingHandler<TEvent>(List<string> log, string name) : IDomainEventHandler<TEvent>
        where TEvent : IDomainEvent
    {
        public Task HandleAsync(TEvent domainEvent, CancellationToken cancellationToken = default)
        {
            log.Add($"{name}:{domainEvent.GetType().Name}");
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task DispatchAsync_CallsEveryRegisteredHandler_ForEvent()
    {
        var log = new List<string>();
        var services = new ServiceCollection();
        services.AddSingleton<IDomainEventHandler<AidRequestedEvent>>(new RecordingHandler<AidRequestedEvent>(log, "H1"));
        services.AddSingleton<IDomainEventHandler<AidRequestedEvent>>(new RecordingHandler<AidRequestedEvent>(log, "H2"));
        services.AddSingleton<IDomainEventHandler<ShelterAssignedEvent>>(new RecordingHandler<ShelterAssignedEvent>(log, "H3"));

        await using var sp = services.BuildServiceProvider();
        var dispatcher = new DomainEventDispatcher(sp);

        var ev = new AidRequestedEvent(Guid.NewGuid(), DateTimeOffset.UtcNow);
        await dispatcher.DispatchAsync([ev]);

        Assert.Equal(2, log.Count);
        Assert.Contains("H1:AidRequestedEvent", log);
        Assert.Contains("H2:AidRequestedEvent", log);
    }

    [Fact]
    public async Task DispatchAsync_DispatchesMultipleEventsInOrder()
    {
        var log = new List<string>();
        var services = new ServiceCollection();
        services.AddSingleton<IDomainEventHandler<AidRequestedEvent>>(new RecordingHandler<AidRequestedEvent>(log, "A"));
        services.AddSingleton<IDomainEventHandler<ShelterAssignedEvent>>(new RecordingHandler<ShelterAssignedEvent>(log, "B"));

        await using var sp = services.BuildServiceProvider();
        var dispatcher = new DomainEventDispatcher(sp);

        var now = DateTimeOffset.UtcNow;
        await dispatcher.DispatchAsync([
            new AidRequestedEvent(Guid.NewGuid(), now),
            new ShelterAssignedEvent(Guid.NewGuid(), now.AddSeconds(1)),
        ]);

        Assert.Equal(["A:AidRequestedEvent", "B:ShelterAssignedEvent"], log);
    }

    [Fact]
    public async Task DispatchAsync_Succeeds_WhenNoHandlersRegistered()
    {
        var services = new ServiceCollection();
        await using var sp = services.BuildServiceProvider();
        var dispatcher = new DomainEventDispatcher(sp);

        await dispatcher.DispatchAsync([new AidRequestedEvent(Guid.NewGuid(), DateTimeOffset.UtcNow)]);
    }
}
