using CrisisPlatform.Api.Modules.Auth.Tokens;

namespace CrisisPlatform.Tests.Security;

internal sealed class InMemoryRefreshTokenStore : IRefreshTokenStore
{
    public Dictionary<string, RefreshTokenRecord> Records { get; } = new();

    public Task<RefreshTokenRecord?> FindByHashAsync(string tokenHash, CancellationToken cancellationToken = default)
        => Task.FromResult(Records.GetValueOrDefault(tokenHash));

    public Task AddAsync(RefreshTokenRecord record, CancellationToken cancellationToken = default)
    {
        Records[record.TokenHash] = record;
        return Task.CompletedTask;
    }

    public Task SaveAsync(RefreshTokenRecord record, CancellationToken cancellationToken = default)
    {
        Records[record.TokenHash] = record;
        return Task.CompletedTask;
    }

    public Task RevokeFamilyAsync(string familyId, DateTimeOffset revokedAt, CancellationToken cancellationToken = default)
    {
        foreach (var record in Records.Values.Where(r => r.FamilyId == familyId && r.RevokedAt is null))
        {
            record.RevokedAt = revokedAt;
        }

        return Task.CompletedTask;
    }
}
