using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace CrisisPlatform.Api.Modules.Mapping;

/// <summary>Geographic coordinate.</summary>
public sealed record GeoPoint(double Latitude, double Longitude);

/// <summary>Result of a route distance and ETA calculation.</summary>
public sealed record RouteEstimate(
    double DistanceKm,
    double EtaMinutes,
    bool   UsedFallback);

public interface IRoutingClient
{
    Task<RouteEstimate> EstimateRouteAsync(
        GeoPoint origin,
        GeoPoint destination,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Routing client that queries an OSRM-compatible endpoint and falls back to
/// Haversine straight-line distance (at 40 km/h) when the routing API is unreachable.
/// </summary>
public sealed class RoutingClient(HttpClient httpClient) : IRoutingClient
{
    private const double EarthRadiusKm = 6371.0;
    private const double FallbackSpeedKmh = 40.0;

    public async Task<RouteEstimate> EstimateRouteAsync(
        GeoPoint origin,
        GeoPoint destination,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var url = $"https://router.project-osrm.org/route/v1/driving/{origin.Longitude:F5},{origin.Latitude:F5};{destination.Longitude:F5},{destination.Latitude:F5}?overview=false";
            var payload = await httpClient.GetFromJsonAsync<OsrmResponse>(url, cancellationToken);

            var route = payload?.Routes?.FirstOrDefault();
            if (route is not null && route.DistanceMeters > 0)
            {
                return new RouteEstimate(
                    DistanceKm:   Math.Round(route.DistanceMeters / 1000.0, 2),
                    EtaMinutes:   Math.Round(route.DurationSeconds / 60.0, 1),
                    UsedFallback: false);
            }
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or InvalidOperationException)
        {
            // Fall through to straight-line Haversine estimate.
        }

        var straightKm = HaversineKm(origin, destination);
        var etaMinutes = (straightKm / FallbackSpeedKmh) * 60.0;

        return new RouteEstimate(
            DistanceKm:   Math.Round(straightKm, 2),
            EtaMinutes:   Math.Round(etaMinutes, 1),
            UsedFallback: true);
    }

    public static double HaversineKm(GeoPoint a, GeoPoint b)
    {
        double dLat = ToRadians(b.Latitude - a.Latitude);
        double dLon = ToRadians(b.Longitude - a.Longitude);
        double lat1 = ToRadians(a.Latitude);
        double lat2 = ToRadians(b.Latitude);

        double sinLat = Math.Sin(dLat / 2);
        double sinLon = Math.Sin(dLon / 2);
        double h = sinLat * sinLat + Math.Cos(lat1) * Math.Cos(lat2) * sinLon * sinLon;
        return 2 * EarthRadiusKm * Math.Asin(Math.Min(1.0, Math.Sqrt(h)));
    }

    private static double ToRadians(double degrees) => degrees * (Math.PI / 180.0);

    private sealed record OsrmResponse(
        [property: JsonPropertyName("routes")] IReadOnlyList<OsrmRoute>? Routes);

    private sealed record OsrmRoute(
        [property: JsonPropertyName("distance")] double DistanceMeters,
        [property: JsonPropertyName("duration")] double DurationSeconds);
}
