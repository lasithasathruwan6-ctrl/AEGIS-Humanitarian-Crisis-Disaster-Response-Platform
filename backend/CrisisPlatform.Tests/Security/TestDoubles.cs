using CrisisPlatform.Api.Modules.Auth.Otp;

namespace CrisisPlatform.Tests.Security;

/// <summary>Controllable clock so expiry and cooldown rules can be tested without sleeping.</summary>
internal sealed class ManualTimeProvider(DateTimeOffset start) : TimeProvider
{
    private DateTimeOffset _now = start;

    public override DateTimeOffset GetUtcNow() => _now;

    public void Advance(TimeSpan by) => _now += by;
}

internal sealed class InMemoryOtpStore : IOtpStore
{
    public Dictionary<string, OtpRecord> Records { get; } = new();

    public Task<OtpRecord?> GetAsync(string email, CancellationToken cancellationToken = default)
        => Task.FromResult(Records.GetValueOrDefault(email));

    public Task SaveAsync(OtpRecord record, CancellationToken cancellationToken = default)
    {
        Records[record.Email] = record;
        return Task.CompletedTask;
    }
}
