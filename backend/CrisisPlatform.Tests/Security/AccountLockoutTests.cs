using CrisisPlatform.Api.Modules.Auth.Lockout;

namespace CrisisPlatform.Tests.Security;

public class AccountLockoutTests
{
    private const string User = "user-1";

    private sealed class InMemoryLockoutStore : ILockoutStore
    {
        private readonly Dictionary<string, LockoutState> _states = new();

        public Task<LockoutState> GetAsync(string userId, CancellationToken cancellationToken = default)
            => Task.FromResult(_states.GetValueOrDefault(userId) ?? new LockoutState());

        public Task SaveAsync(string userId, LockoutState state, CancellationToken cancellationToken = default)
        {
            _states[userId] = state;
            return Task.CompletedTask;
        }
    }

    private readonly ManualTimeProvider _clock = new(new DateTimeOffset(2026, 4, 1, 12, 0, 0, TimeSpan.Zero));
    private readonly LockoutOptions _options = new() { MaxFailedAttempts = 3, LockoutMinutes = 15 };

    private AccountLockoutService CreateService() => new(new InMemoryLockoutStore(), _clock, _options);

    [Fact]
    public async Task Account_locks_after_the_configured_number_of_failures()
    {
        var service = CreateService();

        Assert.Null(await service.RegisterFailureAsync(User));
        Assert.Null(await service.RegisterFailureAsync(User));
        var lockedUntil = await service.RegisterFailureAsync(User);

        Assert.Equal(_clock.GetUtcNow().AddMinutes(15), lockedUntil);
        Assert.Equal(lockedUntil, await service.GetLockedUntilAsync(User));
    }

    [Fact]
    public async Task Account_is_not_locked_below_the_limit()
    {
        var service = CreateService();
        await service.RegisterFailureAsync(User);
        await service.RegisterFailureAsync(User);

        Assert.Null(await service.GetLockedUntilAsync(User));
    }

    [Fact]
    public async Task Lock_expires_after_the_lockout_period()
    {
        var service = CreateService();
        for (var i = 0; i < 3; i++)
        {
            await service.RegisterFailureAsync(User);
        }

        _clock.Advance(TimeSpan.FromMinutes(15));

        Assert.Null(await service.GetLockedUntilAsync(User));
    }

    [Fact]
    public async Task Failures_during_a_lock_do_not_extend_it()
    {
        var service = CreateService();
        DateTimeOffset? original = null;
        for (var i = 0; i < 3; i++)
        {
            original = await service.RegisterFailureAsync(User);
        }

        _clock.Advance(TimeSpan.FromMinutes(5));
        var again = await service.RegisterFailureAsync(User);

        Assert.Equal(original, again);
    }

    [Fact]
    public async Task Counter_restarts_after_a_lock_has_elapsed()
    {
        var service = CreateService();
        for (var i = 0; i < 3; i++)
        {
            await service.RegisterFailureAsync(User);
        }

        _clock.Advance(TimeSpan.FromMinutes(16));

        Assert.Null(await service.RegisterFailureAsync(User));
        Assert.Null(await service.GetLockedUntilAsync(User));
    }

    [Fact]
    public async Task Successful_login_resets_the_failure_counter()
    {
        var service = CreateService();
        await service.RegisterFailureAsync(User);
        await service.RegisterFailureAsync(User);

        await service.RegisterSuccessAsync(User);

        Assert.Null(await service.RegisterFailureAsync(User));
        Assert.Null(await service.RegisterFailureAsync(User));
    }

    [Fact]
    public async Task Lockout_is_per_account()
    {
        var service = CreateService();
        for (var i = 0; i < 3; i++)
        {
            await service.RegisterFailureAsync(User);
        }

        Assert.Null(await service.GetLockedUntilAsync("user-2"));
    }

    [Fact]
    public void Non_positive_options_are_rejected()
    {
        Assert.Throws<ArgumentException>(() =>
            new AccountLockoutService(new InMemoryLockoutStore(), _clock, new LockoutOptions { MaxFailedAttempts = 0 }));
    }
}
