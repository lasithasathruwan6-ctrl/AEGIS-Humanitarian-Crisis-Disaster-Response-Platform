using CrisisPlatform.Api.Modules.Auth.Tokens;

namespace CrisisPlatform.Tests.Integration.Helpers;

/// <summary>
/// Fluent builders that construct test-only value objects used across integration tests.
/// All builders live inside the test project and must never be referenced by application code.
/// </summary>

// ---------------------------------------------------------------------------
// TokenOptions builder
// ---------------------------------------------------------------------------

public sealed class TokenOptionsBuilder
{
    private string _signingKey        = "integration-test-signing-key-0123456789-abcdef";
    private int    _accessTokenMinutes = 15;

    public TokenOptionsBuilder WithSigningKey(string key)
    {
        _signingKey = key;
        return this;
    }

    public TokenOptionsBuilder WithAccessTokenMinutes(int minutes)
    {
        _accessTokenMinutes = minutes;
        return this;
    }

    public TokenOptions Build() => new()
    {
        SigningKey         = _signingKey,
        AccessTokenMinutes = _accessTokenMinutes,
    };
}

// ---------------------------------------------------------------------------
// User / principal builder
// ---------------------------------------------------------------------------

public sealed class TestPrincipalBuilder
{
    private string _userId = "test-user-id";
    private string _role   = "Citizen";

    public TestPrincipalBuilder WithUserId(string userId)
    {
        _userId = userId;
        return this;
    }

    public TestPrincipalBuilder WithRole(string role)
    {
        _role = role;
        return this;
    }

    /// <summary>Builds and returns a JWT access-token string signed with the given options.</summary>
    public string BuildAccessToken(TokenOptions? options = null)
    {
        var opts    = options ?? new TokenOptionsBuilder().Build();
        var service = new AccessTokenService(opts, TimeProvider.System);
        return service.Create(_userId, _role);
    }
}

// ---------------------------------------------------------------------------
// Allocation request builder
// ---------------------------------------------------------------------------

public sealed class AllocationRequestBuilder
{
    private string _shelterId = "shelter-default-001";
    private int    _occupants = 1;

    public AllocationRequestBuilder ForShelter(string shelterId)
    {
        _shelterId = shelterId;
        return this;
    }

    public AllocationRequestBuilder WithOccupants(int count)
    {
        _occupants = count;
        return this;
    }

    public (string ShelterId, int Occupants) Build() => (_shelterId, _occupants);
}

// ---------------------------------------------------------------------------
// Citizen request builder
// ---------------------------------------------------------------------------

public sealed class CitizenRequestBuilder
{
    private string _type        = "Medical";
    private string _description = "Test request — integration test fixture";
    private double _latitude    = 6.9271;
    private double _longitude   = 79.8612;

    public CitizenRequestBuilder OfType(string type)
    {
        _type = type;
        return this;
    }

    public CitizenRequestBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    public CitizenRequestBuilder AtLocation(double latitude, double longitude)
    {
        _latitude  = latitude;
        _longitude = longitude;
        return this;
    }

    public object Build() => new
    {
        Type        = _type,
        Description = _description,
        Location    = new { Latitude = _latitude, Longitude = _longitude },
    };
}
