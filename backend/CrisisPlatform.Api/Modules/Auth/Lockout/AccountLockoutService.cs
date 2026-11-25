namespace CrisisPlatform.Api.Modules.Auth.Lockout;

public sealed class LockoutOptions
{
    public const string SectionName = "Auth:Lockout";

    public int MaxFailedAttempts { get; set; } = 5;
    public int LockoutMinutes { get; set; } = 15;
}

public sealed class LockoutState
{
    public int FailedCount { get; set; }
    public DateTimeOffset? LockedUntil { get; set; }
}

public interface ILockoutStore
{
    Task<LockoutState> GetAsync(string userId, CancellationToken cancellationToken = default);

    Task SaveAsync(string userId, LockoutState state, CancellationToken cancellationToken = default);
}

public sealed class AccountLockoutService
{
    private readonly ILockoutStore _store;
    private readonly TimeProvider _clock;
    private readonly LockoutOptions _options;

    public AccountLockoutService(ILockoutStore store, TimeProvider clock, LockoutOptions options)
    {
        if (options.MaxFailedAttempts < 1 || options.LockoutMinutes < 1)
        {
            throw new ArgumentException("Lockout options must be positive.", nameof(options));
        }

        _store = store;
        _clock = clock;
        _options = options;
    }

    /// <summary>Returns the time the lock ends, or null when the account may attempt a login.</summary>
    public async Task<DateTimeOffset?> GetLockedUntilAsync(string userId, CancellationToken cancellationToken = default)
    {
        var state = await _store.GetAsync(userId, cancellationToken);
        return state.LockedUntil is { } until && until > _clock.GetUtcNow() ? until : null;
    }

    /// <summary>Records a failed login and locks the account when the limit is reached.</summary>
    public async Task<DateTimeOffset?> RegisterFailureAsync(string userId, CancellationToken cancellationToken = default)
    {
        var now = _clock.GetUtcNow();
        var state = await _store.GetAsync(userId, cancellationToken);

        if (state.LockedUntil is { } until)
        {
            if (until > now)
            {
                return until; // already locked: further failures do not extend or change the lock
            }

            // The previous lock has elapsed, so start counting afresh.
            state.LockedUntil = null;
            state.FailedCount = 0;
        }

        state.FailedCount++;
        if (state.FailedCount >= _options.MaxFailedAttempts)
        {
            state.LockedUntil = now.AddMinutes(_options.LockoutMinutes);
        }

        await _store.SaveAsync(userId, state, cancellationToken);
        return state.LockedUntil;
    }

    /// <summary>Clears the failure counter after a successful login.</summary>
    public Task RegisterSuccessAsync(string userId, CancellationToken cancellationToken = default)
        => _store.SaveAsync(userId, new LockoutState(), cancellationToken);
}
