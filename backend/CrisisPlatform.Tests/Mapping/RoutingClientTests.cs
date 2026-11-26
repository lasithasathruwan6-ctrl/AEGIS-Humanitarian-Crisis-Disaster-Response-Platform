using System.Net;
using System.Text;
using CrisisPlatform.Api.Modules.Mapping;

namespace CrisisPlatform.Tests.Mapping;

/// <summary>Unit tests for <see cref="RoutingClient"/> including straight-line fallback.</summary>
public sealed class RoutingClientTests
{
    private sealed class MockHandler(Func<HttpRequestMessage, HttpResponseMessage> fn) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(fn(request));
    }

    private static readonly GeoPoint Colombo = new(6.9271, 79.8612);
    private static readonly GeoPoint Kandy   = new(7.2906, 80.6337);

    [Fact]
    public async Task EstimateRouteAsync_UsesExternalRoute_WhenApiSucceeds()
    {
        const string json = """
        {
          "routes": [
            { "distance": 115400.0, "duration": 9300.0 }
          ]
        }
        """;

        using var client = new HttpClient(new MockHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            }));

        var sut = new RoutingClient(client);
        var estimate = await sut.EstimateRouteAsync(Colombo, Kandy);

        Assert.False(estimate.UsedFallback);
        Assert.Equal(115.4, estimate.DistanceKm);
        Assert.Equal(155.0, estimate.EtaMinutes);
    }

    [Fact]
    public async Task EstimateRouteAsync_FallsBackToStraightLine_WhenApiReturns503()
    {
        using var client = new HttpClient(new MockHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)));

        var sut = new RoutingClient(client);
        var estimate = await sut.EstimateRouteAsync(Colombo, Kandy);

        Assert.True(estimate.UsedFallback);
        Assert.InRange(estimate.DistanceKm, 90.0, 105.0);
        Assert.True(estimate.EtaMinutes > 0);
    }

    [Fact]
    public async Task EstimateRouteAsync_FallsBackToStraightLine_WhenHttpThrows()
    {
        using var client = new HttpClient(new MockHandler(_ =>
            throw new HttpRequestException("Network unreachable")));

        var sut = new RoutingClient(client);
        var estimate = await sut.EstimateRouteAsync(Colombo, Kandy);

        Assert.True(estimate.UsedFallback);
        Assert.Equal(Math.Round(RoutingClient.HaversineKm(Colombo, Kandy), 2), estimate.DistanceKm);
    }
}
