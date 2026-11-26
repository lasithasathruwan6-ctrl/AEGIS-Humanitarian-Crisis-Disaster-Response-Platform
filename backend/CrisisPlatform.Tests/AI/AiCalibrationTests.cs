using CrisisPlatform.Api.Modules.AI;

namespace CrisisPlatform.Tests.AI;

public class AiCalibrationTests
{
    [Fact]
    public void ComputeCalibrationBuckets_ShouldGroupByConfidenceRange()
    {
        var decisions = new[]
        {
            new HumanDecisionRecord(DecisionOutcome.Rejected, 0.30),
            new HumanDecisionRecord(DecisionOutcome.Approved, 0.70),
            new HumanDecisionRecord(DecisionOutcome.Modified, 0.74),
            new HumanDecisionRecord(DecisionOutcome.Approved, 0.90),
            new HumanDecisionRecord(DecisionOutcome.Approved, 0.96)
        };

        var buckets = AiGovernanceService.ComputeCalibrationBuckets(decisions);

        Assert.Equal(5, buckets.Count);
        var b2040 = buckets.Single(b => b.RangeLabel == "20-40%");
        Assert.Equal(1, b2040.Count);
        Assert.Equal(0.0, b2040.AgreementRate);

        var b6080 = buckets.Single(b => b.RangeLabel == "60-80%");
        Assert.Equal(2, b6080.Count);
        Assert.Equal(0.5, b6080.AgreementRate);

        var b80100 = buckets.Single(b => b.RangeLabel == "80-100%");
        Assert.Equal(2, b80100.Count);
        Assert.Equal(1.0, b80100.AgreementRate);
    }
}
