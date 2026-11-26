namespace CrisisPlatform.Api.Modules.AI;

public sealed record ChatMessageItem(string SenderRole, string Text);

public sealed record CopilotDraftResult(
    string Summary,
    string SuggestedReply,
    bool IsDraftOnly,
    bool AutoSent);

public sealed class AiCopilotService
{
    private readonly List<string> _sentOutbox = [];
    public IReadOnlyList<string> SentOutbox => _sentOutbox;

    public CopilotDraftResult GenerateSummaryAndSuggestedReply(
        string trackingNumber,
        string area,
        IReadOnlyList<ChatMessageItem> messages)
    {
        var lastCitizen = messages.LastOrDefault(m => m.SenderRole.Equals("Citizen", StringComparison.OrdinalIgnoreCase))?.Text ?? "No citizen message yet.";
        var redacted = PiiRedactor.Redact(lastCitizen);
        var summary = $"Case {trackingNumber} ({area}): {messages.Count} message(s). Latest citizen note: {redacted}";
        var suggestedReply = $"Thank you for contacting AEGIS ({trackingNumber}). A coordinator for {area} is reviewing your request and will confirm dispatch details shortly.";

        // Safety invariant: AI copilot drafts are never auto-sent
        return new CopilotDraftResult(summary, suggestedReply, IsDraftOnly: true, AutoSent: false);
    }
}
