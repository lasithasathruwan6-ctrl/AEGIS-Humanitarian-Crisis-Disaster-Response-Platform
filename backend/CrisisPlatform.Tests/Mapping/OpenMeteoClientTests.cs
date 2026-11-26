using System.Net;
using System.Text;
using CrisisPlatform.Api.Modules.Mapping;

namespace CrisisPlatform.Tests.Mapping;

/// <summary>Unit tests for <see cref="OpenMeteoClient"/> with mocked HTTP responses.</summary>
public sealed class OpenMeteoClientTests
{
    private sealed class MockHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responder)
        : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(responder(request));
    }

    [Fact]
    public async Task GetCurrentWeatherAsync_MapsFieldsCorrectly_ForNormalWeather()
    {
        const string json = """
        {
          "latitude": 6.9271,
          "longitude": 79.8612,
          "current": {
            "temperature_2m": 29.4,
            "precipitation": 2.1,
            "wind_speed_10m": 14.5,
            "weather_code": 3
          }
        }
        """;

        using var httpClient = new HttpClient(new MockHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            }));

        var sut = new OpenMeteoClient(httpClient);
        var snapshot = await sut.GetCurrentWeatherAsync("Colombo", 6.9271, 79.8612);

        Assert.Equal("Colombo", snapshot.District);
        Assert.Equal(29.4, snapshot.TemperatureCelsius);
        Assert.Equal(2.1, snapshot.PrecipitationMm);
        Assert.Equal(14.5, snapshot.WindSpeedKmh);
        Assert.Equal(3, snapshot.WeatherCode);
        Assert.False(snapshot.IsSevere);
    }

    [Theory]
    [InlineData(30.0, 10.0, 61)] // heavy rain >= 25mm
    [InlineData(5.0,  55.0, 3)]  // high wind >= 50km/h
    [InlineData(0.0,  10.0, 95)] // thunderstorm code >= 95
    public async Task GetCurrentWeatherAsync_FlagsSevereWeather(double precip, double wind, int code)
    {
        var json = $$"""
        {
          "current": {
            "temperature_2m": 26.0,
            "precipitation": {{precip}},
            "wind_speed_10m": {{wind}},
            "weather_code": {{code}}
          }
        }
        """;

        using var httpClient = new HttpClient(new MockHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            }));

        var sut = new OpenMeteoClient(httpClient);
        var snapshot = await sut.GetCurrentWeatherAsync("Ratnapura", 6.6828, 80.3992);

        Assert.True(snapshot.IsSevere);
    }

    [Fact]
    public async Task GetCurrentWeatherAsync_Throws_WhenCurrentBlockMissing()
    {
        using var httpClient = new HttpClient(new MockHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{}", Encoding.UTF8, "application/json")
            }));

        var sut = new OpenMeteoClient(httpClient);
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.GetCurrentWeatherAsync("Kandy", 7.2906, 80.6337));
    }
}
