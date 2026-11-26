using CrisisPlatform.Api.Modules.AI;

namespace CrisisPlatform.Tests.AI;

public class AiFairnessTests
{
    [Fact]
    public void ComputeFairnessDisparities_ShouldFlagGroupAboveConfiguredThreshold()
    {
        var samples = new[]
        {
            new FairnessSample("Language", "en", true),
            new FairnessSample("Language", "en", true),
            new FairnessSample("Language", "en", true),
            new FairnessSample("Language", "en", true),
            new FairnessSample("Language", "si", false),
            new FairnessSample("Language", "si", false),
            new FairnessSample("Language", "si", false),
            new FairnessSample("Language", "si", false)
        };

        // Overall mean is 0.50; "en" is 1.00 (disparity 0.50 > 0.15) and "si" is 0.00 (disparity 0.50 > 0.15)
        var report = AiGovernanceService.ComputeFairnessDisparities(samples, disparityThreshold: 0.15);

        Assert.Equal(2, report.Count);
        Assert.All(report, r => Assert.True(r.Flagged));
    }

    [Fact]
    public void ComputeFairnessDisparities_ShouldNotFlagBalancedGroups()
    {
        var samples = new[]
        {
            new FairnessSample("District", "Colombo", true),
            new FairnessSample("District", "Colombo", false),
            new FairnessSample("District", "Kandy", true),
            new FairnessSample("District", "Kandy", false)
        };

        var report = AiGovernanceService.ComputeFairnessDisparities(samples, disparityThreshold: 0.15);

        Assert.All(report, r => Assert.False(r.Flagged));
    }
}
