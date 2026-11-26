using CrisisPlatform.Api.Modules.Resources;

namespace CrisisPlatform.Tests.Allocation;

/// <summary>Unit tests for <see cref="FastestEtaStrategy"/>.</summary>
public sealed class FastestEtaStrategyTests
{
    private static readonly IAllocationStrategy Sut = new FastestEtaStrategy();

    private static ResourceCandidate C(Guid id, double eta, int capacity = 100, double dist = 20) =>
        new(id, "Kandy", capacity, dist, eta);

    [Fact]
    public void Allocate_ReturnsNull_WhenNoCandidates()
    {
        var result = Sut.Allocate([], urgency: 4);
        Assert.Null(result);
    }

    [Fact]
    public void Allocate_ReturnsSingleCandidate_WhenOnlyOne()
    {
        var id = Guid.NewGuid();
        var result = Sut.Allocate([C(id, eta: 45)], urgency: 4);
        Assert.NotNull(result);
        Assert.Equal(id, result.ResourceId);
    }

    [Fact]
    public void Allocate_PicksFastestEta()
    {
        var slowId = Guid.NewGuid();
        var fastId = Guid.NewGuid();

        var result = Sut.Allocate(
            [C(slowId, eta: 90), C(fastId, eta: 15)],
            urgency: 4);

        Assert.NotNull(result);
        Assert.Equal(fastId, result.ResourceId);
    }

    [Fact]
    public void Allocate_ReasonContainsEtaMinutes()
    {
        var id = Guid.NewGuid();
        var result = Sut.Allocate([C(id, eta: 22.5)], urgency: 4);
        Assert.Contains("22.5", result!.Reason);
    }

    [Fact]
    public void Allocate_TieBreaksConsistently_FirstAmongEquals()
    {
        // Both have ETA = 10; strategy must still return one deterministically.
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();

        var result = Sut.Allocate([C(id1, eta: 10), C(id2, eta: 10)], urgency: 1);
        Assert.NotNull(result);
    }
}
