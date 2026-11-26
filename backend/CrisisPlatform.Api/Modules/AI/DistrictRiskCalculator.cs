namespace CrisisPlatform.Api.Modules.AI;

/// <summary>Inputs for computing a district's composite crisis risk index (0–100).</summary>
public sealed record DistrictRiskInput(
    string         District,
    double         PrecipitationMm,
    double         WindSpeedKmh,
    int            ActiveCriticalRequests,
    double         ShelterOccupancyRatio, // 0.0 – 1.0
    DateTimeOffset WeatherObservedAt);

public sealed record DistrictRiskResult(
    string District,
    double RiskIndex,   // 0 – 100
    string RiskBand,    // Low, Moderate, High, Critical
    bool   IsStaleWeather);

/// <summary>
/// Computes a 0–100 district risk index from weather severity, active critical
/// requests, and shelter occupancy, guarding against stale weather observations.
/// </summary>
public sealed class DistrictRiskCalculator(TimeProvider clock, TimeSpan? maxWeatherAge = null)
{
    private readonly TimeSpan _maxWeatherAge = maxWeatherAge ?? TimeSpan.FromHours(3);

    public DistrictRiskResult Calculate(DistrictRiskInput input)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(input.District);

        var now = clock.GetUtcNow();
        bool isStale = (now - input.WeatherObservedAt) > _maxWeatherAge;

        // Stale weather data must not inflate or suppress current operational risk:
        // ignore weather contribution when stale.
        double weatherScore = isStale
            ? 0.0
            : Math.Min(40.0, (input.PrecipitationMm / 50.0) * 25.0 + (input.WindSpeedKmh / 80.0) * 15.0);

        double requestScore = Math.Min(35.0, input.ActiveCriticalRequests * 3.5);
        double shelterScore = Math.Clamp(input.ShelterOccupancyRatio, 0.0, 1.0) * 25.0;

        double total = Math.Round(Math.Clamp(weatherScore + requestScore + shelterScore, 0.0, 100.0), 1);

        string band = total switch
        {
            >= 75.0 => "Critical",
            >= 50.0 => "High",
            >= 25.0 => "Moderate",
            _       => "Low",
        };

        return new DistrictRiskResult(input.District, total, band, isStale);
    }
}
