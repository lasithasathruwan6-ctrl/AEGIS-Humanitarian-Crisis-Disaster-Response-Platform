namespace CrisisPlatform.Api.Modules.AI;

public sealed record DonationAnomalyResult(
    bool IsFlagged,
    string? Reason);

public static class DonationAnomalyDetector
{
    public const decimal DefaultHighValueThreshold = 1_000_000m;
    public const int DefaultRapidRepeatLimit = 3;

    public static DonationAnomalyResult Evaluate(
        decimal amount,
        int recentPledgeCountBySameDonor,
        decimal highValueThreshold = DefaultHighValueThreshold,
        int rapidRepeatLimit = DefaultRapidRepeatLimit)
    {
        var reasons = new List<string>();
        if (amount >= highValueThreshold)
            reasons.Add($"High-value pledge (>= {highValueThreshold:N0} LKR)");
        if (recentPledgeCountBySameDonor >= rapidRepeatLimit)
            reasons.Add($"Rapid-repeat donor activity ({recentPledgeCountBySameDonor} pledges in window)");

        if (reasons.Count == 0)
            return new DonationAnomalyResult(false, null);

        return new DonationAnomalyResult(true, string.Join("; ", reasons));
    }
}
