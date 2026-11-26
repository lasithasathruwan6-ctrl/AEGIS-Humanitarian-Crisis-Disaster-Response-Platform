using CrisisPlatform.Api.Modules.Mapping;

namespace CrisisPlatform.Api.Modules.Requests;

public sealed record DuplicateCandidate(
    Guid           Id,
    string         NeedType,
    string         Description,
    GeoPoint       Location,
    DateTimeOffset CreatedAt);

/// <summary>
/// Detects duplicate aid requests by combining Jaccard token similarity,
/// Haversine geographic proximity, and a configurable time window.
/// </summary>
public sealed class DuplicateRequestDetector(
    double minTokenSimilarity = 0.6,
    double maxDistanceKm = 1.5,
    TimeSpan? timeWindow = null)
{
    private readonly TimeSpan _window = timeWindow ?? TimeSpan.FromHours(6);

    public bool IsDuplicate(DuplicateCandidate incoming, DuplicateCandidate existing)
    {
        if (!string.Equals(incoming.NeedType, existing.NeedType, StringComparison.OrdinalIgnoreCase))
            return false;

        if ((incoming.CreatedAt - existing.CreatedAt).Duration() > _window)
            return false;

        if (RoutingClient.HaversineKm(incoming.Location, existing.Location) > maxDistanceKm)
            return false;

        return ComputeJaccardSimilarity(incoming.Description, existing.Description) >= minTokenSimilarity;
    }

    public static double ComputeJaccardSimilarity(string a, string b)
    {
        var tokensA = Tokenize(a);
        var tokensB = Tokenize(b);
        if (tokensA.Count == 0 && tokensB.Count == 0) return 1.0;
        if (tokensA.Count == 0 || tokensB.Count == 0) return 0.0;

        int intersection = tokensA.Intersect(tokensB).Count();
        int union        = tokensA.Union(tokensB).Count();
        return union == 0 ? 0.0 : (double)intersection / union;
    }

    private static HashSet<string> Tokenize(string text) =>
        text.Split([' ', '\t', '\r', '\n', ',', '.', ';', '!', '?'], StringSplitOptions.RemoveEmptyEntries)
            .Select(t => t.Trim().ToLowerInvariant())
            .Where(t => t.Length > 1)
            .ToHashSet(StringComparer.Ordinal);
}
