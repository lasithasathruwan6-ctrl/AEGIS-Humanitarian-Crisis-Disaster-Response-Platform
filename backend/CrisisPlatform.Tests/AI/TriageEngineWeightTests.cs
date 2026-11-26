using CrisisPlatform.Api.Modules.AI;

namespace CrisisPlatform.Tests.AI;

public class TriageEngineWeightTests
{
    [Fact]
    public void DefaultWeights_ShouldSumTo100Percent()
    {
        var weights = TriageEngine.DefaultWeights;
        Assert.Equal(1.00, weights.Total, precision: 4);
    }

    [Fact]
    public void HigherSelfReportedUrgency_ShouldRankHigher_WhenOtherFieldsEqual()
    {
        var engine = new TriageEngine();
        var lowUrgency = engine.Evaluate(new TriageInput("Water", "Need clean drinking water for family.", 1, 3));
        var highUrgency = engine.Evaluate(new TriageInput("Water", "Need clean drinking water for family.", 5, 3));

        Assert.True(highUrgency.UrgencyScore > lowUrgency.UrgencyScore);
    }
}
