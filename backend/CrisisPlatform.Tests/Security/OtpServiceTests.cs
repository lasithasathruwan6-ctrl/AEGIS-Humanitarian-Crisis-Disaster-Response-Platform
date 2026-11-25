using System.Security.Cryptography;
using CrisisPlatform.Api.Modules.Auth.Otp;

namespace CrisisPlatform.Tests.Security;

public class OtpServiceTests
{
    private const string Email = "citizen@example.org";

    private readonly InMemoryOtpStore _store = new();
    private readonly ManualTimeProvider _clock = new(new DateTimeOffset(2026, 1, 1, 8, 0, 0, TimeSpan.Zero));
    private readonly OtpOptions _options = new() { Length = 6, LifetimeMinutes = 10, MaxAttempts = 3 };

    private OtpService CreateService() => new(_store, _clock, _options, RandomNumberGenerator.GetBytes(32));

    [Fact]
    public async Task Code_is_stored_only_as_a_hash()
    {
        var service = CreateService();

        var issued = await service.IssueAsync(Email);

        var record = _store.Records[Email];
        Assert.Equal(6, issued.Code!.Length);
        Assert.DoesNotContain(issued.Code, record.CodeHash);
        Assert.NotEqual(issued.Code, record.CodeHash);
        Assert.Equal(64, record.CodeHash.Length); // hex HMAC-SHA256
    }

    [Fact]
    public async Task Same_code_for_different_emails_produces_different_hashes()
    {
        var service = CreateService();
        var first = await service.IssueAsync("a@example.org");
        var second = await service.IssueAsync("b@example.org");

        Assert.NotEqual(_store.Records["a@example.org"].CodeHash, _store.Records["b@example.org"].CodeHash);
        Assert.NotNull(first.Code);
        Assert.NotNull(second.Code);
    }

    [Fact]
    public async Task Correct_code_verifies_once_only()
    {
        var service = CreateService();
        var issued = await service.IssueAsync(Email);

        Assert.Equal(OtpVerifyResult.Success, await service.VerifyAsync(Email, issued.Code!));
        Assert.Equal(OtpVerifyResult.NotFound, await service.VerifyAsync(Email, issued.Code!));
    }

    [Fact]
    public async Task Email_is_matched_case_insensitively()
    {
        var service = CreateService();
        var issued = await service.IssueAsync("Citizen@Example.org ");

        Assert.Equal(OtpVerifyResult.Success, await service.VerifyAsync(" CITIZEN@example.ORG", issued.Code!));
    }

    [Fact]
    public async Task Code_expires_after_the_configured_lifetime()
    {
        var service = CreateService();
        var issued = await service.IssueAsync(Email);

        _clock.Advance(TimeSpan.FromMinutes(10));

        Assert.Equal(OtpVerifyResult.Expired, await service.VerifyAsync(Email, issued.Code!));
    }

    [Fact]
    public async Task Code_is_still_valid_just_before_expiry()
    {
        var service = CreateService();
        var issued = await service.IssueAsync(Email);

        _clock.Advance(TimeSpan.FromMinutes(9) + TimeSpan.FromSeconds(59));

        Assert.Equal(OtpVerifyResult.Success, await service.VerifyAsync(Email, issued.Code!));
    }

    [Fact]
    public async Task Attempts_are_limited_and_the_correct_code_is_then_refused()
    {
        var service = CreateService();
        var issued = await service.IssueAsync(Email);
        var wrong = issued.Code == "000000" ? "111111" : "000000";

        Assert.Equal(OtpVerifyResult.Invalid, await service.VerifyAsync(Email, wrong));
        Assert.Equal(OtpVerifyResult.Invalid, await service.VerifyAsync(Email, wrong));
        Assert.Equal(OtpVerifyResult.TooManyAttempts, await service.VerifyAsync(Email, wrong));
        Assert.Equal(OtpVerifyResult.TooManyAttempts, await service.VerifyAsync(Email, issued.Code!));
    }

    [Fact]
    public async Task Unknown_email_is_reported_as_not_found()
    {
        var service = CreateService();

        Assert.Equal(OtpVerifyResult.NotFound, await service.VerifyAsync("nobody@example.org", "123456"));
    }

    [Fact]
    public async Task Issuing_a_new_code_invalidates_the_previous_one()
    {
        var service = CreateService();
        var first = await service.IssueAsync(Email);
        _clock.Advance(TimeSpan.FromSeconds(_options.ResendCooldownSeconds));
        var second = await service.IssueAsync(Email);

        if (first.Code != second.Code)
        {
            Assert.Equal(OtpVerifyResult.Invalid, await service.VerifyAsync(Email, first.Code!));
        }

        Assert.Equal(OtpVerifyResult.Success, await service.VerifyAsync(Email, second.Code!));
    }

    [Fact]
    public void Short_hash_key_is_rejected()
    {
        Assert.Throws<ArgumentException>(() => new OtpService(_store, _clock, _options, new byte[16]));
    }
}
