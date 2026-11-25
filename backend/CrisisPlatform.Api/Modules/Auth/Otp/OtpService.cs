using System.Security.Cryptography;
using System.Text;

namespace CrisisPlatform.Api.Modules.Auth.Otp;

public sealed class OtpOptions
{
    public const string SectionName = "Auth:Otp";

    public int Length { get; set; } = 6;
    public int LifetimeMinutes { get; set; } = 10;
    public int MaxAttempts { get; set; } = 5;

    /// <summary>Minimum time between two codes sent to the same address.</summary>
    public int ResendCooldownSeconds { get; set; } = 60;
}

public sealed class OtpRecord
{
    public required string Email { get; init; }

    /// <summary>HMAC of the code; the plain code is never persisted.</summary>
    public required string CodeHash { get; init; }

    public required DateTimeOffset CreatedAt { get; init; }
    public required DateTimeOffset ExpiresAt { get; init; }
    public int Attempts { get; set; }
    public bool Consumed { get; set; }
}

/// <summary>Persistence port: holds at most one active OTP per email address.</summary>
public interface IOtpStore
{
    Task<OtpRecord?> GetAsync(string email, CancellationToken cancellationToken = default);

    Task SaveAsync(OtpRecord record, CancellationToken cancellationToken = default);
}

public enum OtpIssueStatus
{
    Issued,
    Cooldown,
}

/// <param name="Code">The plain code to deliver by email; only set when <see cref="Status"/> is Issued.</param>
/// <param name="RetryAfter">How long the caller must wait; only set when <see cref="Status"/> is Cooldown.</param>
public sealed record OtpIssueResult(OtpIssueStatus Status, string? Code, TimeSpan? RetryAfter = null);

public enum OtpVerifyResult
{
    Success,
    Invalid,
    Expired,
    TooManyAttempts,
    NotFound,
}

public sealed class OtpService
{
    private readonly IOtpStore _store;
    private readonly TimeProvider _clock;
    private readonly OtpOptions _options;
    private readonly byte[] _hashKey;

    public OtpService(IOtpStore store, TimeProvider clock, OtpOptions options, byte[] hashKey)
    {
        if (hashKey.Length < 32)
        {
            throw new ArgumentException("OTP hash key must be at least 32 bytes.", nameof(hashKey));
        }

        if (options.Length < 6 || options.LifetimeMinutes < 1 || options.MaxAttempts < 1 || options.ResendCooldownSeconds < 0)
        {
            throw new ArgumentException("OTP options are out of range.", nameof(options));
        }

        _store = store;
        _clock = clock;
        _options = options;
        _hashKey = hashKey;
    }

    public async Task<OtpIssueResult> IssueAsync(string email, CancellationToken cancellationToken = default)
    {
        email = Normalise(email);
        var now = _clock.GetUtcNow();

        var previous = await _store.GetAsync(email, cancellationToken);
        if (previous is not null)
        {
            var retryAt = previous.CreatedAt.AddSeconds(_options.ResendCooldownSeconds);
            if (now < retryAt)
            {
                return new OtpIssueResult(OtpIssueStatus.Cooldown, null, retryAt - now);
            }
        }

        var code = RandomNumberGenerator.GetInt32(0, (int)Math.Pow(10, _options.Length))
            .ToString().PadLeft(_options.Length, '0');

        await _store.SaveAsync(new OtpRecord
        {
            Email = email,
            CodeHash = Hash(email, code),
            CreatedAt = now,
            ExpiresAt = now.AddMinutes(_options.LifetimeMinutes),
        }, cancellationToken);

        return new OtpIssueResult(OtpIssueStatus.Issued, code);
    }

    public async Task<OtpVerifyResult> VerifyAsync(string email, string code, CancellationToken cancellationToken = default)
    {
        email = Normalise(email);
        var record = await _store.GetAsync(email, cancellationToken);

        if (record is null || record.Consumed)
        {
            return OtpVerifyResult.NotFound;
        }

        if (_clock.GetUtcNow() >= record.ExpiresAt)
        {
            return OtpVerifyResult.Expired;
        }

        if (record.Attempts >= _options.MaxAttempts)
        {
            return OtpVerifyResult.TooManyAttempts;
        }

        var supplied = Encoding.UTF8.GetBytes(Hash(email, code ?? string.Empty));
        var expected = Encoding.UTF8.GetBytes(record.CodeHash);

        if (!CryptographicOperations.FixedTimeEquals(supplied, expected))
        {
            record.Attempts++;
            await _store.SaveAsync(record, cancellationToken);
            return record.Attempts >= _options.MaxAttempts ? OtpVerifyResult.TooManyAttempts : OtpVerifyResult.Invalid;
        }

        record.Consumed = true;
        await _store.SaveAsync(record, cancellationToken);
        return OtpVerifyResult.Success;
    }

    private static string Normalise(string email) => email.Trim().ToLowerInvariant();

    private string Hash(string email, string code)
        => Convert.ToHexString(HMACSHA256.HashData(_hashKey, Encoding.UTF8.GetBytes($"{email}:{code}")));
}
