namespace CrisisPlatform.Api.Modules.AI;

public sealed record TriageWeights(
    double UrgencyWeight = 0.40,
    double VulnerabilityWeight = 0.25,
    double KeywordSeverityWeight = 0.20,
    double HouseholdWeight = 0.15)
{
    public double Total => Math.Round(UrgencyWeight + VulnerabilityWeight + KeywordSeverityWeight + HouseholdWeight, 4);
}

public sealed record TriageInput(
    string Category,
    string Description,
    int SelfReportedUrgency,
    int PeopleCount,
    string VulnerableFlags = "",
    string Language = "en");

public sealed record TriageResult(
    int UrgencyScore,
    string Priority,
    double Confidence,
    bool NeedsHumanReview,
    bool HasCriticalKeyword,
    IReadOnlyList<string> MatchedKeywords,
    string CounterfactualNote);

public sealed class TriageEngine
{
    public static readonly TriageWeights DefaultWeights = new();

    private static readonly string[] CriticalKeywords =
    [
        // English
        "trapped", "drowning", "bleeding", "unconscious", "insulin", "oxygen", "pregnant", "infant", "collapse", "landslide", "flood", "urgent",
        // Sinhala
        "ගංවතුර", "නායයෑම්", "හදිසි", "රෝගී", "ගැබිනි", "හුස්ම", "බේරාගන්න",
        // Tamil
        "வெள்ளம்", "மண்சரிவு", "அவசர", "மருத்துவ", "கர்ப்பிணி", "உயிர்",
        // Singlish
        "gilenawa", "udaw", "maranaya", "wathura", "leda", "hadisi"
    ];

    public TriageResult Evaluate(TriageInput input, double confidenceThreshold = 0.60, TriageWeights? weights = null)
    {
        var w = weights ?? DefaultWeights;
        if (Math.Abs(w.Total - 1.0) > 0.0001)
            throw new ArgumentException($"Scoring weights must sum to 1.0 (got {w.Total}).");

        var text = (input.Description ?? string.Empty).Trim();
        var lower = text.ToLowerInvariant();
        var matched = CriticalKeywords.Where(k => lower.Contains(k, StringComparison.OrdinalIgnoreCase)).Distinct().ToList();
        var hasCritical = matched.Count > 0;

        var urgencyNorm = Math.Clamp(input.SelfReportedUrgency, 1, 5) / 5.0;
        var vulnFlags = (input.VulnerableFlags ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var vulnNorm = Math.Min(1.0, vulnFlags.Length / 2.0);
        var keywordNorm = hasCritical ? Math.Min(1.0, 0.70 + matched.Count * 0.10) : 0.15;
        var householdNorm = Math.Min(1.0, Math.Max(1, input.PeopleCount) / 8.0);

        var score = (int)Math.Round(100.0 * (
            urgencyNorm * w.UrgencyWeight +
            vulnNorm * w.VulnerabilityWeight +
            keywordNorm * w.KeywordSeverityWeight +
            householdNorm * w.HouseholdWeight));

        score = Math.Clamp(score, 0, 100);

        var priority = score switch
        {
            >= 80 => "Critical",
            >= 60 => "High",
            >= 35 => "Medium",
            _ => "Low"
        };

        // Safety invariant: critical keywords never lower priority below High
        if (hasCritical && priority is "Low" or "Medium")
        {
            priority = "High";
            score = Math.Max(score, 60);
        }

        // Confidence rises with clearer text & explicit metadata; sparse descriptions yield lower confidence
        var confidence = 0.42;
        if (text.Length >= 20) confidence += 0.18;
        if (text.Length >= 50) confidence += 0.12;
        if (hasCritical) confidence += 0.15;
        if (vulnFlags.Length > 0) confidence += 0.08;
        confidence = Math.Round(Math.Min(0.98, confidence), 2);

        var needsReview = confidence < confidenceThreshold;
        var counterfactual = BuildCounterfactual(priority, hasCritical, vulnFlags.Length > 0, input.SelfReportedUrgency);

        return new TriageResult(score, priority, confidence, needsReview, hasCritical, matched, counterfactual);
    }

    private static string BuildCounterfactual(string priority, bool hasCritical, bool hasVulnerability, int urgency)
    {
        if (priority == "Critical")
            return hasCritical
                ? "Priority would drop to High or Medium if critical hazard/medical keywords were absent."
                : "Priority would drop one tier if self-reported urgency were below 4 or no vulnerable flags were present.";
        if (priority == "High")
            return hasVulnerability
                ? "Priority would drop to Medium if vulnerable household flags and critical keywords were absent."
                : "Priority would drop to Medium if urgency were 2 or lower without critical keywords.";
        if (priority == "Medium")
            return urgency >= 3
                ? "Priority would rise to High if critical hazard keywords or vulnerable household flags were present."
                : "Priority would drop to Low if household size were 1 and urgency were 1.";
        return "Priority would rise to Medium or High if critical keywords, vulnerable flags, or higher urgency were reported.";
    }
}
