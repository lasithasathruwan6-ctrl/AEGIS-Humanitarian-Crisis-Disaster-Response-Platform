using System.Security.Cryptography;
using CrisisPlatform.Api.Modules.Auth.Otp;

namespace CrisisPlatform.Tests.Security;

public class OtpResendCooldownTests
{
    private const string Email = "citizen@example.org";

    private readonly InMemoryOtpStore _store = new();
    private readonly ManualTimeProvider _clock = new(new DateTimeOffset(2026, 1, 1, 8, 0, 0, TimeSpan.Zero));
    private readonly OtpOptions _options = new() { ResendCooldownSeconds = 60, LifetimeMinutes = 10 };

    private OtpService CreateService() => new(_store, _clock, _options, RandomNumberGenerator.GetBytes(32));

    [Fact]
    public async Task Resend_inside_the_cooldown_is_refused_and_keeps_the_original_code()
    {
        var service = CreateService();
        var first = await service.IssueAsync(Email);
        var originalHash = _store.Records[Email].CodeHash;

        _clock.Advance(TimeSpan.FromSeconds(20));
        var second = await service.IssueAsync(Email);

        Assert.Equal(OtpIssueStatus.Cooldown, second.Status);
        Assert.Null(second.Code);
        Assert.Equal(TimeSpan.FromSeconds(40), second.RetryAfter);
        Assert.Equal(originalHash, _store.Records[Email].CodeHash);
        Assert.Equal(OtpVerifyResult.Success, await service.VerifyAsync(Email, first.Code!));
    }

    [Fact]
    public async Task Resend_after_the_cooldown_issues_a_new_code()
    {
        var service = CreateService();
        await service.IssueAsync(Email);

        _clock.Advance(TimeSpan.FromSeconds(60));
        var second = await service.IssueAsync(Email);

        Assert.Equal(OtpIssueStatus.Issued, second.Status);
        Assert.NotNull(second.Code);
    }

    [Fact]
    public async Task Cooldown_is_tracked_per_email_address()
    {
        var service = CreateService();
        await service.IssueAsync("a@example.org");

        var other = await service.IssueAsync("b@example.org");

        Assert.Equal(OtpIssueStatus.Issued, other.Status);
    }

    [Fact]
    public async Task Cooldown_applies_regardless_of_email_casing()
    {
        var service = CreateService();
        await service.IssueAsync(Email);

        var again = await service.IssueAsync(" CITIZEN@Example.org");

        Assert.Equal(OtpIssueStatus.Cooldown, again.Status);
    }
}
