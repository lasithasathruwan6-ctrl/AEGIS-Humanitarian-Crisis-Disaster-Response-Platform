using CrisisPlatform.Api.Modules.AI;

namespace CrisisPlatform.Tests.AI;

public class TriageCriticalKeywordTests
{
    [Theory]
    [InlineData("Child is drowning near the canal bridge.")]
    [InlineData("Elderly patient needs insulin and oxygen immediately.")]
    [InlineData("Pregnant mother trapped after landslide behind house.")]
    public void CriticalKeywords_ShouldNeverLowerPriorityBelowHigh(string description)
    {
        var engine = new TriageEngine();
        // Even with lowest self-reported urgency (1) and household size (1), critical keywords enforce >= High
        var result = engine.Evaluate(new TriageInput("Medical", description, SelfReportedUrgency: 1, PeopleCount: 1));

        Assert.True(result.HasCriticalKeyword);
        Assert.Contains(result.Priority, new[] { "High", "Critical" });
        Assert.True(result.UrgencyScore >= 60);
    }
}
