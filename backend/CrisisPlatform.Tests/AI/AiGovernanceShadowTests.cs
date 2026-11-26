using CrisisPlatform.Api.Modules.AI;

namespace CrisisPlatform.Tests.AI;

public class AiGovernanceShadowTests
{
    [Fact]
    public void ShadowMode_ShouldScoreSilentlyAndHideRecommendationsFromStaff()
    {
        var gov = new AiGovernanceService();
        gov.SwitchMode(AiOperatingMode.Shadow, "admin-1", "Shadow mode verification");

        var eval = gov.ProcessIntake(new TriageInput("Medical", "Trapped in flood water, need insulin.", 5, 3, "medical"));

        Assert.False(eval.RecommendationVisibleToStaff);
        Assert.NotNull(eval.Recommendation);
        Assert.Equal("ManualTriage", eval.RouteStatus);
    }
}
