namespace CrisisPlatform.Api.Modules.AI;

public enum AiOperatingMode
{
    Active,
    Shadow,
    Off
}

public enum DecisionOutcome
{
    Approved,
    Modified,
    Rejected,
    Deferred
}

public sealed record AiModeAuditEntry(
    AiOperatingMode PreviousMode,
    AiOperatingMode NewMode,
    string ActorId,
    string Reason,
    DateTime TimestampUtc);

public sealed record IntakeEvaluation(
    string RouteStatus,
    bool RecommendationVisibleToStaff,
    TriageResult? Recommendation);

public sealed record HumanDecisionRecord(
    DecisionOutcome Outcome,
    double AiConfidence);

public sealed record OverrideAnalytics(
    int TotalDecisions,
    int Approved,
    int Modified,
    int Rejected,
    int Deferred,
    double AgreementRate);

public sealed record CalibrationBucket(
    string RangeLabel,
    int Count,
    double AvgConfidence,
    double AgreementRate);

public sealed record FairnessSample(
    string Dimension,
    string Group,
    bool IsHighOrCritical);

public sealed record FairnessGroupReport(
    string Dimension,
    string Group,
    int TotalCount,
    double HighOrCriticalRate,
    double DisparityFromMean,
    bool Flagged);

public sealed class AiGovernanceService
{
    private readonly TriageEngine _triage = new();
    private readonly List<AiModeAuditEntry> _auditLog = [];

    public AiOperatingMode CurrentMode { get; private set; } = AiOperatingMode.Active;
    public double ConfidenceThreshold { get; private set; } = 0.60;
    public double FairnessDisparityThreshold { get; private set; } = 0.15;
    public IReadOnlyList<AiModeAuditEntry> AuditLog => _auditLog;

    public void SwitchMode(AiOperatingMode newMode, string actorId, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason) || reason.Trim().Length < 5)
            throw new ArgumentException("Audit reason must be at least 5 characters.", nameof(reason));

        var prev = CurrentMode;
        CurrentMode = newMode;
        _auditLog.Add(new AiModeAuditEntry(prev, newMode, actorId, reason.Trim(), DateTime.UtcNow));
    }

    public IntakeEvaluation ProcessIntake(TriageInput input)
    {
        if (CurrentMode == AiOperatingMode.Off)
        {
            return new IntakeEvaluation("ManualTriage", RecommendationVisibleToStaff: false, Recommendation: null);
        }

        var scored = _triage.Evaluate(input, ConfidenceThreshold);
        if (CurrentMode == AiOperatingMode.Shadow)
        {
            return new IntakeEvaluation("ManualTriage", RecommendationVisibleToStaff: false, Recommendation: scored);
        }

        var route = scored.NeedsHumanReview ? "NeedsReview" : "Triaged";
        return new IntakeEvaluation(route, RecommendationVisibleToStaff: true, Recommendation: scored);
    }

    public static OverrideAnalytics ComputeOverrideAnalytics(IEnumerable<HumanDecisionRecord> decisions)
    {
        var list = decisions.ToList();
        var total = list.Count;
        if (total == 0)
            return new OverrideAnalytics(0, 0, 0, 0, 0, 0.0);

        var approved = list.Count(d => d.Outcome == DecisionOutcome.Approved);
        var modified = list.Count(d => d.Outcome == DecisionOutcome.Modified);
        var rejected = list.Count(d => d.Outcome == DecisionOutcome.Rejected);
        var deferred = list.Count(d => d.Outcome == DecisionOutcome.Deferred);
        var agreement = Math.Round((double)approved / total, 4);

        return new OverrideAnalytics(total, approved, modified, rejected, deferred, agreement);
    }

    public static IReadOnlyList<CalibrationBucket> ComputeCalibrationBuckets(IEnumerable<HumanDecisionRecord> decisions)
    {
        var list = decisions.ToList();
        var bounds = new (string Label, double Min, double Max)[]
        {
            ("0-20%", 0.0, 0.2),
            ("20-40%", 0.2, 0.4),
            ("40-60%", 0.4, 0.6),
            ("60-80%", 0.6, 0.8),
            ("80-100%", 0.8, 1.01)
        };

        var buckets = new List<CalibrationBucket>();
        foreach (var (label, min, max) in bounds)
        {
            var inBucket = list.Where(d => d.AiConfidence >= min && d.AiConfidence < max).ToList();
            if (inBucket.Count == 0)
            {
                buckets.Add(new CalibrationBucket(label, 0, 0.0, 0.0));
                continue;
            }
            var avgConf = Math.Round(inBucket.Average(d => d.AiConfidence), 4);
            var agree = Math.Round((double)inBucket.Count(d => d.Outcome == DecisionOutcome.Approved) / inBucket.Count, 4);
            buckets.Add(new CalibrationBucket(label, inBucket.Count, avgConf, agree));
        }
        return buckets;
    }

    public static IReadOnlyList<FairnessGroupReport> ComputeFairnessDisparities(
        IEnumerable<FairnessSample> samples,
        double disparityThreshold)
    {
        var list = samples.ToList();
        if (list.Count == 0) return [];

        var overallMean = list.Average(s => s.IsHighOrCritical ? 1.0 : 0.0);
        return list
            .GroupBy(s => (s.Dimension, s.Group))
            .Select(g =>
            {
                var rate = Math.Round(g.Average(x => x.IsHighOrCritical ? 1.0 : 0.0), 4);
                var disparity = Math.Round(Math.Abs(rate - overallMean), 4);
                return new FairnessGroupReport(
                    g.Key.Dimension,
                    g.Key.Group,
                    g.Count(),
                    rate,
                    disparity,
                    disparity > disparityThreshold);
            })
            .ToList();
    }
}
