using CrisisPlatform.Api.Modules.AI;
using CrisisPlatform.Tests.Security;

namespace CrisisPlatform.Tests.AI;

/// <summary>Unit tests for <see cref="DistrictRiskCalculator"/> and stale-data guard.</summary>
public sealed class DistrictRiskCalculatorTests
{
    [Fact]
    public void Calculate_CombinesWeatherRequestsAndShelterOccupancy()
    {
        var now = new DateTimeOffset(2026, 6, 1, 12, 0, 0, TimeSpan.Zero);
        var sut = new DistrictRiskCalculator(new ManualTimeProvider(now));

        var result = sut.Calculate(new DistrictRiskInput(
            District:               "Ratnapura",
            PrecipitationMm:        50.0,  // 25 pts
            WindSpeedKmh:           80.0,  // 15 pts -> 40 weather
            ActiveCriticalRequests: 10,    // 35 pts
            ShelterOccupancyRatio:  1.0,   // 25 pts
            WeatherObservedAt:      now.AddMinutes(-30)));

        Assert.False(result.IsStaleWeather);
        Assert.Equal(100.0, result.RiskIndex);
        Assert.Equal("Critical", result.RiskBand);
    }

    [Fact]
    public void Calculate_DropsWeatherContribution_WhenWeatherDataIsStale()
    {
        var now = new DateTimeOffset(2026, 6, 1, 12, 0, 0, TimeSpan.Zero);
        var sut = new DistrictRiskCalculator(new ManualTimeProvider(now), TimeSpan.FromHours(3));

        var result = sut.Calculate(new DistrictRiskInput(
            District:               "Galle",
            PrecipitationMm:        50.0,
            WindSpeedKmh:           80.0,
            ActiveCriticalRequests: 0,
            ShelterOccupancyRatio:  0.4, // 10 pts
            WeatherObservedAt:      now.AddHours(-4)));

        Assert.True(result.IsStaleWeather);
        Assert.Equal(10.0, result.RiskIndex);
        Assert.Equal("Low", result.RiskBand);
    }
}
