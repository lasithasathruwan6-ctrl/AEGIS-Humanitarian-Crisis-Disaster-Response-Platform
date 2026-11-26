using System.Net;

namespace CrisisPlatform.Api.Common.Resilience;

/// <summary>Resilience policy settings for an external HTTP client.</summary>
public sealed class ExternalClientResilienceOptions
{
    public const string SectionName = "ExternalClients";

    public int TimeoutSeconds        { get; set; } = 5;
    public int MaxRetries            { get; set; } = 3;
    public int BaseDelayMs           { get; set; } = 200;
    public int CircuitBreakerFailureThreshold { get; set; } = 5;
    public int CircuitBreakerBreakSeconds     { get; set; } = 30;
}

/// <summary>Thrown when an external client's circuit breaker is open.</summary>
public sealed class CircuitBreakerOpenException(string clientName)
    : HttpRequestException($"Circuit breaker is open for external client '{clientName}'.");

/// <summary>
/// Delegating handler applying per-attempt timeout, exponential-backoff retries
/// on transient HTTP errors (5xx / 408 / network / timeout), and a thread-safe
/// circuit breaker for every external HTTP client.
/// </summary>
public sealed class ResilienceHttpHandler(
    string clientName,
    ExternalClientResilienceOptions options,
    TimeProvider clock) : DelegatingHandler
{
    private readonly object _gate = new();
    private int _consecutiveFailures;
    private DateTimeOffset? _openUntil;

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        EnsureCircuitClosed();

        Exception? lastException = null;
        HttpResponseMessage? lastResponse = null;

        for (int attempt = 0; attempt <= options.MaxRetries; attempt++)
        {
            if (attempt > 0)
            {
                var delay = TimeSpan.FromMilliseconds(options.BaseDelayMs * Math.Pow(2, attempt - 1));
                await Task.Delay(delay, clock, cancellationToken);
            }

            using var attemptCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            attemptCts.CancelAfter(TimeSpan.FromSeconds(options.TimeoutSeconds));

            try
            {
                lastResponse?.Dispose();
                var response = await base.SendAsync(request, attemptCts.Token);

                if (!IsTransientStatus(response.StatusCode))
                {
                    RecordSuccess();
                    return response;
                }

                lastResponse = response;
            }
            catch (Exception ex) when (ex is HttpRequestException or OperationCanceledException && !cancellationToken.IsCancellationRequested)
            {
                lastException = ex;
            }
        }

        RecordFailure();

        if (lastResponse is not null)
            return lastResponse;

        throw lastException ?? new HttpRequestException($"External call to '{clientName}' failed after {options.MaxRetries + 1} attempts.");
    }

    private void EnsureCircuitClosed()
    {
        lock (_gate)
        {
            if (_openUntil is { } until && clock.GetUtcNow() < until)
                throw new CircuitBreakerOpenException(clientName);

            if (_openUntil is not null)
            {
                _openUntil = null;
                _consecutiveFailures = 0;
            }
        }
    }

    private void RecordSuccess()
    {
        lock (_gate)
        {
            _consecutiveFailures = 0;
            _openUntil = null;
        }
    }

    private void RecordFailure()
    {
        lock (_gate)
        {
            _consecutiveFailures++;
            if (_consecutiveFailures >= options.CircuitBreakerFailureThreshold)
                _openUntil = clock.GetUtcNow().AddSeconds(options.CircuitBreakerBreakSeconds);
        }
    }

    private static bool IsTransientStatus(HttpStatusCode code) =>
        (int)code >= 500 || code == HttpStatusCode.RequestTimeout || code == HttpStatusCode.TooManyRequests;
}

/// <summary>DI registration helpers for resilient typed HTTP clients.</summary>
public static class ResilienceExtensions
{
    public static IHttpClientBuilder AddResilientExternalClient<TClient, TImplementation>(
        this IServiceCollection services,
        string clientName,
        IConfiguration configuration)
        where TClient : class
        where TImplementation : class, TClient
    {
        var section = configuration.GetSection($"{ExternalClientResilienceOptions.SectionName}:{clientName}");
        var options = section.Get<ExternalClientResilienceOptions>() ?? new ExternalClientResilienceOptions();

        return services.AddHttpClient<TClient, TImplementation>(clientName)
            .AddHttpMessageHandler(sp => new ResilienceHttpHandler(
                clientName,
                options,
                sp.GetService<TimeProvider>() ?? TimeProvider.System));
    }
}
