using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace CrisisPlatform.Api.Modules.Auth.Tokens;

public sealed class TokenOptions
{
    public const string SectionName = "Auth:Tokens";

    public string Issuer { get; set; } = "crisis-platform";
    public string Audience { get; set; } = "crisis-platform-clients";

    /// <summary>HMAC signing secret; supplied from user-secrets or the environment, never committed.</summary>
    public string SigningKey { get; set; } = string.Empty;

    public int AccessTokenMinutes { get; set; } = 15;
    public int RefreshTokenDays { get; set; } = 7;

    public void Validate()
    {
        if (Encoding.UTF8.GetByteCount(SigningKey) < 32)
        {
            throw new InvalidOperationException($"'{SectionName}:SigningKey' must be at least 32 bytes.");
        }

        if (AccessTokenMinutes is < 1 or > 30)
        {
            throw new InvalidOperationException($"'{SectionName}:AccessTokenMinutes' must be between 1 and 30.");
        }

        if (RefreshTokenDays is < 1 or > 30)
        {
            throw new InvalidOperationException($"'{SectionName}:RefreshTokenDays' must be between 1 and 30.");
        }
    }

    public SymmetricSecurityKey CreateKey() => new(Encoding.UTF8.GetBytes(SigningKey));
}

public sealed class AccessTokenService
{
    private readonly TokenOptions _options;
    private readonly TimeProvider _clock;
    private readonly JsonWebTokenHandler _handler = new();

    public AccessTokenService(TokenOptions options, TimeProvider clock)
    {
        options.Validate();
        _options = options;
        _clock = clock;
    }

    public string Create(string userId, string role)
    {
        var now = _clock.GetUtcNow().UtcDateTime;

        return _handler.CreateToken(new SecurityTokenDescriptor
        {
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            IssuedAt = now,
            NotBefore = now,
            Expires = now.AddMinutes(_options.AccessTokenMinutes),
            Claims = new Dictionary<string, object>
            {
                [JwtRegisteredClaimNames.Sub] = userId,
                [ClaimTypes.Role] = role,
                [JwtRegisteredClaimNames.Jti] = Guid.NewGuid().ToString("N"),
            },
            SigningCredentials = new SigningCredentials(_options.CreateKey(), SecurityAlgorithms.HmacSha256),
        });
    }

    public TokenValidationParameters CreateValidationParameters() => new()
    {
        ValidIssuer = _options.Issuer,
        ValidAudience = _options.Audience,
        IssuerSigningKey = _options.CreateKey(),
        ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
        ValidateLifetime = true,
        RequireExpirationTime = true,
        ClockSkew = TimeSpan.Zero,
    };

    public Task<TokenValidationResult> ValidateAsync(string token)
        => _handler.ValidateTokenAsync(token, CreateValidationParameters());
}
