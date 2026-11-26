using CrisisPlatform.Api.Modules.AI;

namespace CrisisPlatform.Tests.AI;

public class AiGovernanceKillSwitchTests
{
    [Fact]
    public void OffModeKillSwitch_ShouldRouteNewRequestsToManualTriageWithoutAi()
    {
        var gov = new AiGovernanceService();
        gov.SwitchMode(AiOperatingMode.Off, "admin-1", "Pause AI triage during model review");

        var eval = gov.ProcessIntake(new TriageInput("Water", "Need clean drinking water urgently.", 5, 4));

        Assert.Equal("ManualTriage", eval.RouteStatus);
        Assert.False(eval.RecommendationVisibleToStaff);
        Assert.Null(eval.Recommendation);
    }
}
