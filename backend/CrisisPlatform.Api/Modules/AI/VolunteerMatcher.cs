namespace CrisisPlatform.Api.Modules.AI;

public sealed record VolunteerCandidate(
    Guid Id,
    string FullName,
    IReadOnlyList<string> Skills,
    double Latitude,
    double Longitude,
    bool IsVerified = true);

public sealed record VolunteerMatchResult(
    Guid VolunteerId,
    string FullName,
    int MatchScore,
    double DistanceKm,
    string Explanation);

public static class VolunteerMatcher
{
    public static IReadOnlyList<VolunteerMatchResult> RankCandidates(
        IEnumerable<VolunteerCandidate> candidates,
        IReadOnlyList<string> requiredSkills,
        double targetLat,
        double targetLon)
    {
        var reqSet = new HashSet<string>(requiredSkills.Select(s => s.Trim()), StringComparer.OrdinalIgnoreCase);

        return candidates
            .Where(c => c.IsVerified)
            .Select(c =>
            {
                var matchedSkills = c.Skills.Where(s => reqSet.Contains(s.Trim())).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
                var skillRatio = reqSet.Count == 0 ? 1.0 : (double)matchedSkills.Count / reqSet.Count;
                var distKm = Math.Round(HaversineKm(targetLat, targetLon, c.Latitude, c.Longitude), 2);
                var proximityScore = Math.Max(0.0, 1.0 - (distKm / 100.0));
                var totalScore = (int)Math.Round((skillRatio * 70.0) + (proximityScore * 30.0));

                var skillDesc = matchedSkills.Count > 0 ? string.Join(", ", matchedSkills) : "none";
                var explanation = $"Matched skills ({matchedSkills.Count}/{reqSet.Count}: {skillDesc}) · {distKm} km away.";
                return new VolunteerMatchResult(c.Id, c.FullName, totalScore, distKm, explanation);
            })
            .OrderByDescending(r => r.MatchScore)
            .ThenBy(r => r.DistanceKm)
            .ToList();
    }

    private static double HaversineKm(double lat1, double lon1, double lat2, double lon2)
    {
        const double r = 6371.0;
        var dLat = (lat2 - lat1) * Math.PI / 180.0;
        var dLon = (lon2 - lon1) * Math.PI / 180.0;
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
              + Math.Cos(lat1 * Math.PI / 180.0) * Math.Cos(lat2 * Math.PI / 180.0)
              * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        return 2 * r * Math.Asin(Math.Min(1.0, Math.Sqrt(a)));
    }
}
