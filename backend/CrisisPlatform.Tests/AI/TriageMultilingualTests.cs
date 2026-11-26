using CrisisPlatform.Api.Modules.AI;

namespace CrisisPlatform.Tests.AI;

public class TriageMultilingualTests
{
    [Theory]
    [InlineData("si", "ගංවතුර නිසා නිවස යටවී ඇත. හදිසි උපකාර අවශ්‍යයි.", "ගංවතුර")]
    [InlineData("ta", "வெள்ளம் காரணமாக குடும்பம் சிக்கியுள்ளது. அவசர மருத்துவ உதவி தேவை.", "வெள்ளம்")]
    [InlineData("en", "Ape gedara wathura gilenawa, ikmanata udaw karanna hadisi.", "gilenawa")]
    public void MultilingualKeywords_SinhalaTamilSinglish_ShouldBeDetectedAndPrioritized(
        string language, string description, string expectedKeyword)
    {
        var engine = new TriageEngine();
        var result = engine.Evaluate(new TriageInput("Transport", description, 3, 4, Language: language));

        Assert.True(result.HasCriticalKeyword);
        Assert.Contains(expectedKeyword, result.MatchedKeywords);
        Assert.Contains(result.Priority, new[] { "High", "Critical" });
    }
}
