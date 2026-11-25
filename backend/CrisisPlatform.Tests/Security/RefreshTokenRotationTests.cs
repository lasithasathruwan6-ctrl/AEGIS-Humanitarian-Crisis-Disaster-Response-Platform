using CrisisPlatform.Api.Modules.Auth.Tokens;

namespace CrisisPlatform.Tests.Security;

public class RefreshTokenRotationTests
{
    private readonly InMemoryRefreshTokenStore _store = new();
    private readonly ManualTimeProvider _clock = new(new DateTimeOffset(2026, 3, 1, 9, 0, 0, TimeSpan.Zero));
    private readonly TokenOptions _options = new() { RefreshTokenDays = 7 };

    private RefreshTokenService CreateService() => new(_store, _clock, _options);

    [Fact]
    public async Task Rotation_returns_a_different_token_for_the_same_user()
    {
        var service = CreateService();
        var first = await service.IssueAsync("user-1");

        var result = await service.RotateAsync(first);

        Assert.Equal(RefreshStatus.Rotated, result.Status);
        Assert.Equal("user-1", result.UserId);
        Assert.NotEqual(first, result.NewToken);
    }

    [Fact]
    public async Task Rotated_chain_keeps_working_token_by_token()
    {
        var service = CreateService();
        var token = await service.IssueAsync("user-1");

        for (var i = 0; i < 3; i++)
        {
            var result = await service.RotateAsync(token);
            Assert.Equal(RefreshStatus.Rotated, result.Status);
            token = result.NewToken!;
        }
    }

    [Fact]
    public async Task Old_token_cannot_be_used_after_rotation_and_revokes_the_whole_session()
    {
        var service = CreateService();
        var first = await service.IssueAsync("user-1");
        var second = (await service.RotateAsync(first)).NewToken!;

        var replay = await service.RotateAsync(first);
        var afterReplay = await service.RotateAsync(second);

        Assert.Equal(RefreshStatus.ReuseDetected, replay.Status);
        Assert.Equal(RefreshStatus.Revoked, afterReplay.Status);
    }

    [Fact]
    public async Task Sessions_are_independent_so_reuse_in_one_does_not_revoke_another()
    {
        var service = CreateService();
        var phone = await service.IssueAsync("user-1");
        var laptop = await service.IssueAsync("user-1");
        await service.RotateAsync(phone);

        await service.RotateAsync(phone); // replay on the phone session

        Assert.Equal(RefreshStatus.Rotated, (await service.RotateAsync(laptop)).Status);
    }

    [Fact]
    public async Task Refresh_token_expires_after_the_configured_days()
    {
        var service = CreateService();
        var token = await service.IssueAsync("user-1");

        _clock.Advance(TimeSpan.FromDays(7));

        Assert.Equal(RefreshStatus.Expired, (await service.RotateAsync(token)).Status);
    }

    [Fact]
    public async Task Unknown_token_is_invalid()
    {
        var service = CreateService();

        Assert.Equal(RefreshStatus.Invalid, (await service.RotateAsync("not-a-real-token")).Status);
    }
}
