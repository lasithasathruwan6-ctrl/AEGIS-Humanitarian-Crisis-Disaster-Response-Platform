using CrisisPlatform.Api.Modules.AI;

namespace CrisisPlatform.Tests.AI;

public class AssistantInputLimitTests
{
    [Fact]
    public async Task AskAssistant_WhenInputExceedsLimit_ShouldFailValidation()
    {
        var client = new GroqAssistantClient(new HttpClient());
        var oversized = new string('a', GroqAssistantClient.DefaultMaxInputChars + 1);

        var res = await client.AskAssistantAsync(oversized);

        Assert.False(res.IsValid);
        Assert.Contains("maximum length", res.ValidationError, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task AskAssistant_WhenInputIsWithinLimit_ShouldPassValidation()
    {
        var client = new GroqAssistantClient(new HttpClient());
        var valid = new string('a', 120);

        var res = await client.AskAssistantAsync(valid);

        Assert.True(res.IsValid);
        Assert.Null(res.ValidationError);
    }
}
