using CrisisPlatform.Api.Modules.AI;

namespace CrisisPlatform.Tests.AI;

public class TriageConfidenceGateTests
{
    [Fact]
    public void ConfidenceBelowThreshold_ShouldFlagNeedsHumanReview()
    {
        var engine = new TriageEngine();
        // Sparse description without critical keywords or vulnerable flags -> confidence 0.42 < 0.60
        var result = engine.Evaluate(new TriageInput("Food", "Need help", 2, 1), confidenceThreshold: 0.60);

        Assert.True(result.Confidence < 0.60);
        Assert.True(result.NeedsHumanReview);
    }

    [Fact]
    public void ConfidenceAtOrAboveThreshold_ShouldNotFlagNeedsHumanReview()
    {
        var engine = new TriageEngine();
        var result = engine.Evaluate(
            new TriageInput("Medical", "Family trapped by rising flood water; elderly patient needs insulin immediately.", 5, 4, "elderly,medical"),
            confidenceThreshold: 0.60);

        Assert.True(result.Confidence >= 0.60);
        Assert.False(result.NeedsHumanReview);
    }
}
