namespace CrisisPlatform.Api.Modules.Requests;

// ---------------------------------------------------------------------------
// DTOs
// ---------------------------------------------------------------------------

/// <summary>Payload sent by a citizen when submitting an aid request.</summary>
public sealed record CreateRequestDto(
    string NicHash,
    string District,
    string NeedType,
    int    Urgency,          // 1 (low) – 5 (critical)
    string Description,
    string ContactPhone);

/// <summary>Read-model returned to callers.</summary>
public sealed record RequestDto(
    Guid   Id,
    string District,
    string NeedType,
    int    Urgency,
    string Status,
    DateTimeOffset CreatedAt);
