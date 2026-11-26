namespace CrisisPlatform.Api.Modules.Resources;

/// <summary>An aid-resource candidate that can be allocated to a request.</summary>
public sealed record ResourceCandidate(
    Guid   Id,
    string District,
    int    Capacity,
    double DistanceKm,
    double EtaMinutes);

/// <summary>Result of running an allocation strategy.</summary>
public sealed record AllocationResult(
    Guid   ResourceId,
    string Reason);

/// <summary>
/// Strategy interface for selecting the best resource for an aid request.
/// Implementations: <see cref="MaxUrgencyStrategy"/>, <see cref="FairShareStrategy"/>,
/// <see cref="FastestEtaStrategy"/>.
/// </summary>
public interface IAllocationStrategy
{
    /// <summary>Picks the best resource from <paramref name="candidates"/> for a request of given urgency.</summary>
    AllocationResult? Allocate(IReadOnlyList<ResourceCandidate> candidates, int urgency);
}

// ---------------------------------------------------------------------------
// MaxUrgency strategy: picks the candidate with the greatest remaining capacity
// (reserves large-capacity assets for the highest-urgency requests).
// ---------------------------------------------------------------------------

/// <summary>
/// Allocates the resource with the highest remaining capacity.
/// Suitable when urgency is critical (5) and the greatest supply must be deployed.
/// </summary>
public sealed class MaxUrgencyStrategy : IAllocationStrategy
{
    public AllocationResult? Allocate(IReadOnlyList<ResourceCandidate> candidates, int urgency)
    {
        if (candidates.Count == 0) return null;

        var best = candidates.MaxBy(c => c.Capacity)!;
        return new AllocationResult(best.Id, $"MaxUrgency: highest capacity {best.Capacity}");
    }
}

// ---------------------------------------------------------------------------
// FairShare strategy: distributes load evenly – picks the nearest candidate
// whose capacity is at least (averageCapacity / 2) to avoid starving districts.
// ---------------------------------------------------------------------------

/// <summary>
/// Allocates the nearest resource whose capacity is at least half the group average,
/// spreading aid evenly across districts.
/// </summary>
public sealed class FairShareStrategy : IAllocationStrategy
{
    public AllocationResult? Allocate(IReadOnlyList<ResourceCandidate> candidates, int urgency)
    {
        if (candidates.Count == 0) return null;

        double avg = candidates.Average(c => c.Capacity);
        double threshold = avg / 2;

        var eligible = candidates
            .Where(c => c.Capacity >= threshold)
            .OrderBy(c => c.DistanceKm)
            .FirstOrDefault();

        if (eligible is null) return null;
        return new AllocationResult(eligible.Id, $"FairShare: nearest within capacity threshold {threshold:F0}");
    }
}

// ---------------------------------------------------------------------------
// FastestEta strategy: picks the candidate that can arrive first.
// ---------------------------------------------------------------------------

/// <summary>
/// Allocates the resource with the smallest estimated travel time (ETA).
/// Used when speed of arrival is paramount regardless of capacity.
/// </summary>
public sealed class FastestEtaStrategy : IAllocationStrategy
{
    public AllocationResult? Allocate(IReadOnlyList<ResourceCandidate> candidates, int urgency)
    {
        if (candidates.Count == 0) return null;

        var fastest = candidates.MinBy(c => c.EtaMinutes)!;
        return new AllocationResult(fastest.Id, $"FastestEta: ETA {fastest.EtaMinutes} min");
    }
}
