using CrisisPlatform.Api.Common.Errors;
using CrisisPlatform.Api.Common.Pagination;

namespace CrisisPlatform.Api.Modules.Requests;

// ---------------------------------------------------------------------------
// In-memory stand-in for the real persistence layer.
// Replace with an EF Core repository when the DbContext is wired up.
// ---------------------------------------------------------------------------
public sealed class AidRequest
{
    public Guid   Id          { get; set; } = Guid.NewGuid();
    public string NicHash     { get; set; } = string.Empty;
    public string District    { get; set; } = string.Empty;
    public string NeedType    { get; set; } = string.Empty;
    public int    Urgency     { get; set; }
    public string Description { get; set; } = string.Empty;
    public string ContactPhone{ get; set; } = string.Empty;
    public string Status      { get; set; } = "Pending";
    public DateTimeOffset CreatedAt { get; set; }
}

/// <summary>
/// Application-layer service that encapsulates all aid-request business logic.
/// Controllers delegate to this class; they contain no business rules themselves.
/// </summary>
public sealed class RequestService(TimeProvider clock)
{
    // Simulated in-memory store (replaced by IRepository<AidRequest> in production).
    private readonly List<AidRequest> _store = [];
    private readonly Dictionary<string, (string Fingerprint, RequestDto Response)> _idempotencyCache =
        new(StringComparer.Ordinal);

    // -----------------------------------------------------------------------
    // Business rules
    // -----------------------------------------------------------------------

    private static readonly IReadOnlySet<string> ValidNeedTypes =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Food", "Water", "Medical", "Shelter", "Evacuation", "Other"
        };

    /// <summary>
    /// Creates a new aid request after applying domain validation rules and
    /// enforcing <c>Idempotency-Key</c> semantics.
    /// </summary>
    public RequestDto Create(CreateRequestDto dto, string idempotencyKey)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
            throw new ArgumentException("Idempotency-Key must not be empty.", nameof(idempotencyKey));

        // Business rule 1 – urgency must be in range.
        if (dto.Urgency is < 1 or > 5)
            throw new ArgumentOutOfRangeException(nameof(dto.Urgency), "Urgency must be 1–5.");

        // Business rule 2 – need type must be from the controlled vocabulary.
        if (!ValidNeedTypes.Contains(dto.NeedType))
            throw new ArgumentException($"Unknown NeedType '{dto.NeedType}'.", nameof(dto.NeedType));

        var fingerprint = $"{dto.NicHash}|{dto.District}|{dto.NeedType}|{dto.Urgency}|{dto.Description}|{dto.ContactPhone}";

        // Business rule 3 – idempotency: same key + same payload → return cached response;
        // same key + different payload → 409 Conflict.
        if (_idempotencyCache.TryGetValue(idempotencyKey, out var cached))
        {
            if (!string.Equals(cached.Fingerprint, fingerprint, StringComparison.Ordinal))
                throw new IdempotencyConflictException(
                    $"Idempotency-Key '{idempotencyKey}' was already used with a different request payload.");

            return cached.Response;
        }

        var entity = new AidRequest
        {
            NicHash      = dto.NicHash,
            District     = dto.District,
            NeedType     = dto.NeedType,
            Urgency      = dto.Urgency,
            Description  = dto.Description,
            ContactPhone = dto.ContactPhone,
            CreatedAt    = clock.GetUtcNow(),
        };

        _store.Add(entity);
        var response = MapToDto(entity);
        _idempotencyCache[idempotencyKey] = (fingerprint, response);
        return response;
    }

    /// <summary>Returns all requests, optionally filtered by district.</summary>
    public IReadOnlyList<RequestDto> List(string? district = null)
    {
        var query = _store.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(district))
            query = query.Where(r => r.District.Equals(district, StringComparison.OrdinalIgnoreCase));
        return query.Select(MapToDto).ToList();
    }

    /// <summary>Returns a paginated list of requests, optionally filtered by district.</summary>
    public PagedResult<RequestDto> ListPaged(PaginationQuery pagination, string? district = null) =>
        List(district).ToPagedResult(pagination);

    /// <summary>Returns a single request by ID, or <c>null</c> when not found.</summary>
    public RequestDto? GetById(Guid id)
    {
        var entity = _store.FirstOrDefault(r => r.Id == id);
        return entity is null ? null : MapToDto(entity);
    }

    // -----------------------------------------------------------------------
    // Mapping helper (kept private – callers never see the entity)
    // -----------------------------------------------------------------------
    private static RequestDto MapToDto(AidRequest e) =>
        new(e.Id, e.District, e.NeedType, e.Urgency, e.Status, e.CreatedAt);
}
