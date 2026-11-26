using System.Net;
using System.Net.Http.Json;
using CrisisPlatform.Api.Common.Health;
using CrisisPlatform.Tests.Security;

namespace CrisisPlatform.Tests.Health;

/// <summary>Tests for database, AI, and email <c>ServiceHealth</c> checks.</summary>
public sealed class ServiceHealthTests
{
    [Fact]
    public async Task ServiceHealth_Returns200_WhenAllDependenciesHealthy()
    {
        using var client = await TestHostFactory.CreateClientAsync(
            builder => builder.Services.AddServiceHealthChecks(),
            app => app.MapServiceHealthEndpoints());

        var response = await client.GetAsync("/health/services");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var report = await response.Content.ReadFromJsonAsync<ServiceHealthReport>();
        Assert.NotNull(report);
        Assert.Equal("Healthy", report.OverallStatus);
        Assert.Equal(3, report.Services.Count);
        Assert.Contains(report.Services, s => s.Service == "database" && s.Status == "Healthy");
        Assert.Contains(report.Services, s => s.Service == "ai"       && s.Status == "Healthy");
        Assert.Contains(report.Services, s => s.Service == "email"    && s.Status == "Healthy");
    }

    [Fact]
    public async Task ServiceHealth_Returns200Degraded_WhenAiOrEmailFails()
    {
        using var client = await TestHostFactory.CreateClientAsync(
            builder => builder.Services.AddServiceHealthChecks(
                dbProbe:    _ => Task.FromResult(new DependencyProbeResult(true)),
                aiProbe:    _ => Task.FromResult(new DependencyProbeResult(false, "AI timeout")),
                emailProbe: _ => Task.FromResult(new DependencyProbeResult(false, "SMTP down"))),
            app => app.MapServiceHealthEndpoints());

        var response = await client.GetAsync("/health/services");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var report = await response.Content.ReadFromJsonAsync<ServiceHealthReport>();
        Assert.NotNull(report);
        Assert.Equal("Degraded", report.OverallStatus);
        Assert.Contains(report.Services, s => s.Service == "ai"    && s.Status == "Degraded");
        Assert.Contains(report.Services, s => s.Service == "email" && s.Status == "Degraded");
    }

    [Fact]
    public async Task ServiceHealth_Returns503Unhealthy_WhenDatabaseFails()
    {
        using var client = await TestHostFactory.CreateClientAsync(
            builder => builder.Services.AddServiceHealthChecks(
                dbProbe: _ => Task.FromResult(new DependencyProbeResult(false, "DB unreachable"))),
            app => app.MapServiceHealthEndpoints());

        var response = await client.GetAsync("/health/services");
        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);

        var report = await response.Content.ReadFromJsonAsync<ServiceHealthReport>();
        Assert.NotNull(report);
        Assert.Equal("Unhealthy", report.OverallStatus);
    }
}
