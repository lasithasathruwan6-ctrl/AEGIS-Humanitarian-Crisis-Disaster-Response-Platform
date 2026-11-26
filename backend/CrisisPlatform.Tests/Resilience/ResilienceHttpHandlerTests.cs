using System.Net;
using CrisisPlatform.Api.Common.Resilience;
using CrisisPlatform.Tests.Security;

namespace CrisisPlatform.Tests.Resilience;

/// <summary>Unit tests for <see cref="ResilienceHttpHandler"/> (timeouts, retries, circuit breaker).</summary>
public sealed class ResilienceHttpHandlerTests
{
    private sealed class StubHandler(Func<int, CancellationToken, Task<HttpResponseMessage>> handler)
        : HttpMessageHandler
    {
        public int Calls { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Calls++;
            return handler(Calls, cancellationToken);
        }
    }

    [Fact]
    public async Task SendAsync_RetriesTransient503_AndSucceeds()
    {
        var stub = new StubHandler((attempt, _) => Task.FromResult(
            attempt < 3
                ? new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
                : new HttpResponseMessage(HttpStatusCode.OK)));

        var options = new ExternalClientResilienceOptions
        {
            TimeoutSeconds = 5,
            MaxRetries = 3,
            BaseDelayMs = 1,
            CircuitBreakerFailureThreshold = 5,
            CircuitBreakerBreakSeconds = 30,
        };

        var handler = new ResilienceHttpHandler("TestClient", options, TimeProvider.System)
        {
            InnerHandler = stub,
        };

        using var client = new HttpClient(handler);
        var response = await client.GetAsync("https://example.test/ping");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(3, stub.Calls);
    }

    [Fact]
    public async Task SendAsync_OpensCircuitBreaker_AfterThresholdFailures()
    {
        var clock = new ManualTimeProvider(new DateTimeOffset(2026, 6, 1, 12, 0, 0, TimeSpan.Zero));
        var stub = new StubHandler((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError)));

        var options = new ExternalClientResilienceOptions
        {
            TimeoutSeconds = 5,
            MaxRetries = 0,
            BaseDelayMs = 1,
            CircuitBreakerFailureThreshold = 2,
            CircuitBreakerBreakSeconds = 30,
        };

        var handler = new ResilienceHttpHandler("OpenMeteo", options, clock)
        {
            InnerHandler = stub,
        };

        using var client = new HttpClient(handler);

        await client.GetAsync("https://example.test/1");
        await client.GetAsync("https://example.test/2");

        await Assert.ThrowsAsync<CircuitBreakerOpenException>(() =>
            client.GetAsync("https://example.test/3"));

        // After break duration elapses, circuit closes and allows requests again.
        clock.Advance(TimeSpan.FromSeconds(31));
        var resAfter = await client.GetAsync("https://example.test/4");
        Assert.Equal(HttpStatusCode.InternalServerError, resAfter.StatusCode);
    }
}
