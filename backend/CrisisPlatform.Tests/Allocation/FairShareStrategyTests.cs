using CrisisPlatform.Api.Modules.Resources;

namespace CrisisPlatform.Tests.Allocation;

/// <summary>Unit tests for <see cref="FairShareStrategy"/>.</summary>
public sealed class FairShareStrategyTests
{
    private static readonly IAllocationStrategy Sut = new FairShareStrategy();

    private static ResourceCandidate C(Guid id, int capacity, double dist = 10, double eta = 20) =>
        new(id, "Galle", capacity, dist, eta);

    [Fact]
    public void Allocate_ReturnsNull_WhenNoCandidates()
    {
        var result = Sut.Allocate([], urgency: 3);
        Assert.Null(result);
    }

    [Fact]
    public void Allocate_ReturnsSingleCandidate_AlwaysEligible()
    {
        // A single candidate always meets half-of-itself threshold.
        var id = Guid.NewGuid();
        var result = Sut.Allocate([C(id, capacity: 100)], urgency: 3);
        Assert.NotNull(result);
        Assert.Equal(id, result.ResourceId);
    }

    [Fact]
    public void Allocate_SkipsBelowHalfAverageCapacity()
    {
        // avg = (100 + 10) / 2 = 55; threshold = 27.5
        // candidate with capacity 10 is below threshold, so it should NOT be chosen
        // even if it is closest.
        var tinyId = Guid.NewGuid();
        var okId   = Guid.NewGuid();

        var tiny = new ResourceCandidate(tinyId, "Galle", Capacity: 10,  DistanceKm: 1,  EtaMinutes: 5);
        var ok   = new ResourceCandidate(okId,   "Galle", Capacity: 100, DistanceKm: 50, EtaMinutes: 60);

        var result = Sut.Allocate([tiny, ok], urgency: 3);
        Assert.NotNull(result);
        Assert.Equal(okId, result.ResourceId);
    }

    [Fact]
    public void Allocate_AmongEligible_PicksNearest()
    {
        var nearId = Guid.NewGuid();
        var farId  = Guid.NewGuid();

        var near = new ResourceCandidate(nearId, "Galle", Capacity: 100, DistanceKm: 5,  EtaMinutes: 10);
        var far  = new ResourceCandidate(farId,  "Galle", Capacity: 100, DistanceKm: 50, EtaMinutes: 60);

        var result = Sut.Allocate([far, near], urgency: 3);
        Assert.NotNull(result);
        Assert.Equal(nearId, result.ResourceId);
    }

    [Fact]
    public void Allocate_ReasonContainsFairShare()
    {
        var id = Guid.NewGuid();
        var result = Sut.Allocate([C(id, capacity: 100)], urgency: 2);
        Assert.Contains("FairShare", result!.Reason);
    }
}
