using System.Security.Cryptography;
using System.Text;
using CrisisPlatform.Api.Modules.Auth.Tokens;

namespace CrisisPlatform.Tests.Security;

public class RefreshTokenStorageAndLogoutTests
{
    private readonly InMemoryRefreshTokenStore _store = new();
    private readonly ManualTimeProvider _clock = new(new DateTimeOffset(2026, 3, 1, 9, 0, 0, TimeSpan.Zero));

    private static string Sha256Hex(string value)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));

    private RefreshTokenService CreateService() => new(_store, _clock, new TokenOptions());

    [Fact]
    public async Task Plain_token_is_never_stored_only_its_sha256_hash()
    {
        var service = CreateService();

        var token = await service.IssueAsync("user-1");

        var record = Assert.Single(_store.Records.Values);
        Assert.NotEqual(token, record.TokenHash);
        Assert.DoesNotContain(token, record.TokenHash);
        Assert.Equal(64, record.TokenHash.Length);
        Assert.Equal(Sha256Hex(token), record.TokenHash);
        Assert.DoesNotContain(_store.Records.Keys, key => key == token);
    }

    [Fact]
    public async Task Rotation_stores_hashes_for_every_generation()
    {
        var service = CreateService();
        var first = await service.IssueAsync("user-1");
        var second = (await service.RotateAsync(first)).NewToken!;

        Assert.Equal(2, _store.Records.Count);
        Assert.All(new[] { first, second }, token => Assert.DoesNotContain(token, _store.Records.Keys));
    }

    [Fact]
    public async Task Logout_revokes_the_token_so_it_can_no_longer_be_refreshed()
    {
        var service = CreateService();
        var token = await service.IssueAsync("user-1");

        Assert.True(await service.LogoutAsync(token));

        Assert.Equal(RefreshStatus.Revoked, (await service.RotateAsync(token)).Status);
        Assert.NotNull(_store.Records[Sha256Hex(token)].RevokedAt);
    }

    [Fact]
    public async Task Logout_after_rotation_also_revokes_the_newest_token_of_the_session()
    {
        var service = CreateService();
        var first = await service.IssueAsync("user-1");
        var newest = (await service.RotateAsync(first)).NewToken!;

        await service.LogoutAsync(first);

        Assert.Equal(RefreshStatus.Revoked, (await service.RotateAsync(newest)).Status);
    }

    [Fact]
    public async Task Logout_of_one_session_leaves_other_sessions_active()
    {
        var service = CreateService();
        var phone = await service.IssueAsync("user-1");
        var laptop = await service.IssueAsync("user-1");

        await service.LogoutAsync(phone);

        Assert.Equal(RefreshStatus.Rotated, (await service.RotateAsync(laptop)).Status);
    }

    [Fact]
    public async Task Logout_with_unknown_token_reports_false_and_changes_nothing()
    {
        var service = CreateService();
        await service.IssueAsync("user-1");

        Assert.False(await service.LogoutAsync("unknown"));
        Assert.All(_store.Records.Values, r => Assert.Null(r.RevokedAt));
    }
}
