using CrisisPlatform.Api.Modules.Mapping;
using CrisisPlatform.Api.Modules.Requests;

namespace CrisisPlatform.Tests.Requests;

/// <summary>Unit tests for <see cref="DuplicateRequestDetector"/> (token similarity + distance + time window).</summary>
public sealed class DuplicateRequestDetectorTests
{
    private static readonly DuplicateRequestDetector Sut = new(
        minTokenSimilarity: 0.5,
        maxDistanceKm:      2.0,
        timeWindow:         TimeSpan.FromHours(6));

    private static readonly DateTimeOffset T0 = new(2026, 6, 1, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public void IsDuplicate_ReturnsTrue_WhenSimilarTextNearbyAndWithinWindow()
    {
        var existing = new DuplicateCandidate(
            Guid.NewGuid(), "Water",
            "Need clean drinking water for 10 families near Kaduwela bridge",
            new GeoPoint(6.9350, 79.9840), T0);

        var incoming = new DuplicateCandidate(
            Guid.NewGuid(), "Water",
            "Clean drinking water needed for 10 families near Kaduwela bridge",
            new GeoPoint(6.9360, 79.9850), T0.AddHours(1));

        Assert.True(Sut.IsDuplicate(incoming, existing));
    }

    [Fact]
    public void IsDuplicate_ReturnsFalse_WhenOutsideDistanceThreshold()
    {
        var existing = new DuplicateCandidate(
            Guid.NewGuid(), "Water",
            "Need clean drinking water for families",
            new GeoPoint(6.9271, 79.8612), T0); // Colombo

        var incoming = new DuplicateCandidate(
            Guid.NewGuid(), "Water",
            "Need clean drinking water for families",
            new GeoPoint(7.2906, 80.6337), T0.AddMinutes(30)); // Kandy (~95km)

        Assert.False(Sut.IsDuplicate(incoming, existing));
    }

    [Fact]
    public void IsDuplicate_ReturnsFalse_WhenOutsideTimeWindow()
    {
        var existing = new DuplicateCandidate(
            Guid.NewGuid(), "Water",
            "Need clean drinking water for families",
            new GeoPoint(6.9350, 79.9840), T0);

        var incoming = new DuplicateCandidate(
            Guid.NewGuid(), "Water",
            "Need clean drinking water for families",
            new GeoPoint(6.9351, 79.9841), T0.AddHours(8));

        Assert.False(Sut.IsDuplicate(incoming, existing));
    }

    [Fact]
    public void IsDuplicate_ReturnsFalse_WhenTokensDiffer()
    {
        var existing = new DuplicateCandidate(
            Guid.NewGuid(), "Water",
            "Need clean drinking water for families",
            new GeoPoint(6.9350, 79.9840), T0);

        var incoming = new DuplicateCandidate(
            Guid.NewGuid(), "Water",
            "Emergency baby formula and sanitary supplies urgently requested",
            new GeoPoint(6.9351, 79.9841), T0.AddMinutes(15));

        Assert.False(Sut.IsDuplicate(incoming, existing));
    }
}
