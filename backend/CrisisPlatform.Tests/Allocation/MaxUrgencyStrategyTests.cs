using CrisisPlatform.Api.Modules.Resources;

namespace CrisisPlatform.Tests.Allocation;

/// <summary>Unit tests for <see cref="MaxUrgencyStrategy"/>.</summary>
public sealed class MaxUrgencyStrategyTests
{
    private static readonly IAllocationStrategy Sut = new MaxUrgencyStrategy();

    private static ResourceCandidate C(Guid id, int capacity, double dist = 10, double eta = 20) =>
        new(id, "Colombo", capacity, dist, eta);

    [Fact]
    public void Allocate_ReturnsNull_WhenNoCandidates()
    {
        var result = Sut.Allocate([], urgency: 5);
        Assert.Null(result);
    }

    [Fact]
    public void Allocate_ReturnsSingleCandidate_WhenOnlyOne()
    {
        var id = Guid.NewGuid();
        var result = Sut.Allocate([C(id, capacity: 100)], urgency: 5);
        Assert.NotNull(result);
        Assert.Equal(id, result.ResourceId);
    }

    [Fact]
    public void Allocate_PicksHighestCapacity()
    {
        var lowId  = Guid.NewGuid();
        var highId = Guid.NewGuid();

        var result = Sut.Allocate(
            [C(lowId, capacity: 50), C(highId, capacity: 200)],
            urgency: 5);

        Assert.NotNull(result);
        Assert.Equal(highId, result.ResourceId);
    }

    [Fact]
    public void Allocate_ReasonContainsCapacity()
    {
        var id = Guid.NewGuid();
        var result = Sut.Allocate([C(id, capacity: 300)], urgency: 5);
        Assert.Contains("300", result!.Reason);
    }

    [Fact]
    public void Allocate_WorksForAllUrgencyLevels()
    {
        var id = Guid.NewGuid();
        foreach (var urgency in Enumerable.Range(1, 5))
        {
            var result = Sut.Allocate([C(id, capacity: 100)], urgency);
            Assert.NotNull(result);
        }
    }
}
