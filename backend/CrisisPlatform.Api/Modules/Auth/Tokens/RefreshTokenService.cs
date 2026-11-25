using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;

namespace CrisisPlatform.Api.Modules.Auth.Tokens;

public sealed class RefreshTokenRecord
{
    public required string UserId { get; init; }

    /// <summary>All tokens descending from one login share a family so a replay can revoke the whole chain.</summary>
    public required string FamilyId { get; init; }

    /// <summary>SHA-256 of the token; the plain token is only ever held by the client.</summary>
    public required string TokenHash { get; init; }

    public required DateTimeOffset ExpiresAt { get; init; }
    public DateTimeOffset? RevokedAt { get; set; }
    public string? ReplacedByHash { get; set; }
}

public interface IRefreshTokenStore
{
    Task<RefreshTokenRecord?> FindByHashAsync(string tokenHash, CancellationToken cancellationToken = default);

    Task AddAsync(RefreshTokenRecord record, CancellationToken cancellationToken = default);

    Task SaveAsync(RefreshTokenRecord record, CancellationToken cancellationToken = default);

    Task RevokeFamilyAsync(string familyId, DateTimeOffset revokedAt, CancellationToken cancellationToken = default);
}

public enum RefreshStatus
{
    Rotated,
    Invalid,
    Expired,
    Revoked,
    ReuseDetected,
}

public sealed record RefreshResult(RefreshStatus Status, string? UserId = null, string? NewToken = null);

public sealed class RefreshTokenService(IRefreshTokenStore store, TimeProvider clock, TokenOptions options)
{
    /// <summary>Starts a new session (token family) and returns the plain refresh token.</summary>
    public async Task<string> IssueAsync(string userId, CancellationToken cancellationToken = default)
        => await CreateAsync(userId, Guid.NewGuid().ToString("N"), cancellationToken);

    /// <summary>Exchanges a refresh token for a new one; the presented token can never be used again.</summary>
    public async Task<RefreshResult> RotateAsync(string token, CancellationToken cancellationToken = default)
    {
        var now = clock.GetUtcNow();
        var record = await store.FindByHashAsync(Hash(token), cancellationToken);

        if (record is null)
        {
            return new RefreshResult(RefreshStatus.Invalid);
        }

        if (record.RevokedAt is not null)
        {
            if (record.ReplacedByHash is null)
            {
                return new RefreshResult(RefreshStatus.Revoked);
            }

            // An already-rotated token is being replayed: assume theft and kill the whole session.
            await store.RevokeFamilyAsync(record.FamilyId, now, cancellationToken);
            return new RefreshResult(RefreshStatus.ReuseDetected);
        }

        if (now >= record.ExpiresAt)
        {
            return new RefreshResult(RefreshStatus.Expired);
        }

        var next = await CreateAsync(record.UserId, record.FamilyId, cancellationToken);
        record.RevokedAt = now;
        record.ReplacedByHash = Hash(next);
        await store.SaveAsync(record, cancellationToken);

        return new RefreshResult(RefreshStatus.Rotated, record.UserId, next);
    }

    private async Task<string> CreateAsync(string userId, string familyId, CancellationToken cancellationToken)
    {
        var token = WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(32));

        await store.AddAsync(new RefreshTokenRecord
        {
            UserId = userId,
            FamilyId = familyId,
            TokenHash = Hash(token),
            ExpiresAt = clock.GetUtcNow().AddDays(options.RefreshTokenDays),
        }, cancellationToken);

        return token;
    }

    internal static string Hash(string token)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
