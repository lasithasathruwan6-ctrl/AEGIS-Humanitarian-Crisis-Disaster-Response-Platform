using CrisisPlatform.Api.Modules.AI;

namespace CrisisPlatform.Tests.AI;

public class AiGovernanceAnalyticsTests
{
    [Fact]
    public void ComputeOverrideAnalytics_ShouldCalculateAgreementAndOutcomeCounts()
    {
        var decisions = new[]
        {
            new HumanDecisionRecord(DecisionOutcome.Approved, 0.90),
            new HumanDecisionRecord(DecisionOutcome.Approved, 0.85),
            new HumanDecisionRecord(DecisionOutcome.Approved, 0.80),
            new HumanDecisionRecord(DecisionOutcome.Modified, 0.70),
            new HumanDecisionRecord(DecisionOutcome.Rejected, 0.50),
            new HumanDecisionRecord(DecisionOutcome.Deferred, 0.45)
        };

        var analytics = AiGovernanceService.ComputeOverrideAnalytics(decisions);

        Assert.Equal(6, analytics.TotalDecisions);
        Assert.Equal(3, analytics.Approved);
        Assert.Equal(1, analytics.Modified);
        Assert.Equal(1, analytics.Rejected);
        Assert.Equal(1, analytics.Deferred);
        Assert.Equal(0.50, analytics.AgreementRate, precision: 4);
    }
}
