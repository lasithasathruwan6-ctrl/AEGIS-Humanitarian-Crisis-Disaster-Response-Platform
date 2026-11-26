using CrisisPlatform.Api.Modules.AI;

namespace CrisisPlatform.Tests.AI;

public class VolunteerMatcherTests
{
    [Fact]
    public void RankCandidates_ShouldRankBySkillOverlapAndDistanceWithExplanation()
    {
        var nearMatch = new VolunteerCandidate(Guid.NewGuid(), "Kasun Perera", ["Medical", "FirstAid"], 6.930, 79.865);
        var farMatch = new VolunteerCandidate(Guid.NewGuid(), "Nimal Silva", ["Medical", "FirstAid"], 7.290, 80.630);
        var partialMatch = new VolunteerCandidate(Guid.NewGuid(), "Amal Fernando", ["Driving"], 6.927, 79.861);

        var ranked = VolunteerMatcher.RankCandidates(
            [partialMatch, farMatch, nearMatch],
            ["Medical", "FirstAid"],
            targetLat: 6.9271,
            targetLon: 79.8612);

        Assert.Equal(3, ranked.Count);
        Assert.Equal("Kasun Perera", ranked[0].FullName);
        Assert.Equal("Nimal Silva", ranked[1].FullName);
        Assert.Contains("Matched skills (2/2", ranked[0].Explanation);
        Assert.Contains("km away", ranked[0].Explanation);
    }
}
