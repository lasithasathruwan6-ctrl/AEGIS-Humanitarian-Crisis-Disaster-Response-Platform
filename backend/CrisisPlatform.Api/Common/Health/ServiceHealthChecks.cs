using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace CrisisPlatform.Api.Common.Health;

/// <summary>Probe contract for a downstream dependency reported in <c>ServiceHealth</c>.</summary>
public interface IDependencyProbe
{
    string ServiceName { get; }
    Task<DependencyProbeResult> CheckAsync(CancellationToken cancellationToken = default);
}

public sealed record DependencyProbeResult(bool IsHealthy, string? Detail = null);

/// <summary>Database connectivity probe.</summary>
public sealed class DatabaseHealthCheck(Func<CancellationToken, Task<DependencyProbeResult>> probe) : IHealthCheck
{
    public const string Name = "database";

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var res = await probe(cancellationToken);
        return res.IsHealthy
            ? HealthCheckResult.Healthy(res.Detail ?? "SQL Server connection reachable.")
            : HealthCheckResult.Unhealthy(res.Detail ?? "SQL Server connection failed.");
    }
}

/// <summary>AI triage &amp; risk-scoring service probe.</summary>
public sealed class AiServiceHealthCheck(Func<CancellationToken, Task<DependencyProbeResult>> probe) : IHealthCheck
{
    public const string Name = "ai";

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var res = await probe(cancellationToken);
        return res.IsHealthy
            ? HealthCheckResult.Healthy(res.Detail ?? "AI service responsive.")
            : HealthCheckResult.Degraded(res.Detail ?? "AI service unavailable; fallback heuristics active.");
    }
}

/// <summary>Outbound email feed probe.</summary>
public sealed class EmailFeedHealthCheck(Func<CancellationToken, Task<DependencyProbeResult>> probe) : IHealthCheck
{
    public const string Name = "email";

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var res = await probe(cancellationToken);
        return res.IsHealthy
            ? HealthCheckResult.Healthy(res.Detail ?? "Email feed ready.")
            : HealthCheckResult.Degraded(res.Detail ?? "Email feed degraded; outbox will retry.");
    }
}

/// <summary>Aggregated status payload returned by <c>GET /health/services</c>.</summary>
public sealed record ServiceHealthEntry(string Service, string Status, string? Description, double DurationMs);

public sealed record ServiceHealthReport(string OverallStatus, IReadOnlyList<ServiceHealthEntry> Services);

public static class ServiceHealthExtensions
{
    public static IServiceCollection AddServiceHealthChecks(
        this IServiceCollection services,
        Func<CancellationToken, Task<DependencyProbeResult>>? dbProbe = null,
        Func<CancellationToken, Task<DependencyProbeResult>>? aiProbe = null,
        Func<CancellationToken, Task<DependencyProbeResult>>? emailProbe = null)
    {
        var defaultHealthy = static (CancellationToken _) =>
            Task.FromResult(new DependencyProbeResult(true));

        services.AddHealthChecks()
            .AddCheck(DatabaseHealthCheck.Name, new DatabaseHealthCheck(dbProbe ?? defaultHealthy), tags: ["ready"])
            .AddCheck(AiServiceHealthCheck.Name, new AiServiceHealthCheck(aiProbe ?? defaultHealthy), tags: ["ready"])
            .AddCheck(EmailFeedHealthCheck.Name, new EmailFeedHealthCheck(emailProbe ?? defaultHealthy), tags: ["ready"]);

        return services;
    }

    public static IEndpointRouteBuilder MapServiceHealthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHealthChecks("/health");

        endpoints.MapGet("/health/services", async (HealthCheckService healthCheckService, CancellationToken ct) =>
        {
            var report = await healthCheckService.CheckHealthAsync(ct);
            var entries = report.Entries
                .Select(kvp => new ServiceHealthEntry(
                    kvp.Key,
                    kvp.Value.Status.ToString(),
                    kvp.Value.Description,
                    kvp.Value.Duration.TotalMilliseconds))
                .ToList();

            var body = new ServiceHealthReport(report.Status.ToString(), entries);
            var statusCode = report.Status == HealthStatus.Unhealthy
                ? StatusCodes.Status503ServiceUnavailable
                : StatusCodes.Status200OK;

            return Results.Json(body, statusCode: statusCode);
        })
        .WithName("GetServiceHealth")
        .WithTags("Health");

        return endpoints;
    }
}
