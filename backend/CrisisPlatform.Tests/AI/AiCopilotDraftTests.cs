using CrisisPlatform.Api.Modules.AI;

namespace CrisisPlatform.Tests.AI;

public class AiCopilotDraftTests
{
    [Fact]
    public void GenerateSummaryAndSuggestedReply_ShouldBeDraftOnlyAndNeverAutoSend()
    {
        var copilot = new AiCopilotService();
        var messages = new[]
        {
            new ChatMessageItem("Citizen", "We need drinking water near Kalutara bridge. Call 0771234567.")
        };

        var draft = copilot.GenerateSummaryAndSuggestedReply("AEG-2026-1001", "Kalutara", messages);

        Assert.True(draft.IsDraftOnly);
        Assert.False(draft.AutoSent);
        Assert.Empty(copilot.SentOutbox);
        Assert.DoesNotContain("0771234567", draft.Summary);
        Assert.Contains("AEG-2026-1001", draft.SuggestedReply);
    }
}
