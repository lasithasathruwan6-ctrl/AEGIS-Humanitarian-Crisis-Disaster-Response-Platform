using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace CrisisPlatform.Api.Modules.Mapping;

/// <summary>Normalised weather snapshot for a Sri Lankan district.</summary>
public sealed record DistrictWeatherSnapshot(
    string District,
    double TemperatureCelsius,
    double PrecipitationMm,
    double WindSpeedKmh,
    int    WeatherCode,
    bool   IsSevere);

public interface IOpenMeteoClient
{
    Task<DistrictWeatherSnapshot> GetCurrentWeatherAsync(
        string district,
        double latitude,
        double longitude,
        CancellationToken cancellationToken = default);
}

/// <summary>HTTP client mapping Open-Meteo <c>/v1/forecast</c> responses.</summary>
public sealed class OpenMeteoClient(HttpClient httpClient) : IOpenMeteoClient
{
    public async Task<DistrictWeatherSnapshot> GetCurrentWeatherAsync(
        string district,
        double latitude,
        double longitude,
        CancellationToken cancellationToken = default)
    {
        var url = $"https://api.open-meteo.com/v1/forecast?latitude={latitude:F4}&longitude={longitude:F4}&current=temperature_2m,precipitation,wind_speed_10m,weather_code";
        var payload = await httpClient.GetFromJsonAsync<OpenMeteoResponse>(url, cancellationToken)
            ?? throw new InvalidOperationException("Open-Meteo returned an empty payload.");

        if (payload.Current is null)
            throw new InvalidOperationException("Open-Meteo response missing 'current' block.");

        var c = payload.Current;
        bool isSevere = c.Precipitation >= 25.0 || c.WindSpeed10m >= 50.0 || c.WeatherCode >= 95;

        return new DistrictWeatherSnapshot(
            District:           district,
            TemperatureCelsius: c.Temperature2m,
            PrecipitationMm:    c.Precipitation,
            WindSpeedKmh:       c.WindSpeed10m,
            WeatherCode:        c.WeatherCode,
            IsSevere:           isSevere);
    }

    private sealed record OpenMeteoResponse(
        [property: JsonPropertyName("current")] OpenMeteoCurrent? Current);

    private sealed record OpenMeteoCurrent(
        [property: JsonPropertyName("temperature_2m")] double Temperature2m,
        [property: JsonPropertyName("precipitation")]  double Precipitation,
        [property: JsonPropertyName("wind_speed_10m")] double WindSpeed10m,
        [property: JsonPropertyName("weather_code")]   int    WeatherCode);
}
