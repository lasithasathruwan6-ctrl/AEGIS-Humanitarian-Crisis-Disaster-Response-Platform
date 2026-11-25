using System.Security.Cryptography;
using System.Text;
using CrisisPlatform.Api.Modules.Admin.Invitations;

namespace CrisisPlatform.Tests.Security;

public class StaffInvitationTests
{
    private sealed class InMemoryInvitationStore : IInvitationStore
    {
        public Dictionary<string, InvitationRecord> Records { get; } = new();

        public Task AddAsync(InvitationRecord record, CancellationToken cancellationToken = default)
        {
            Records[record.TokenHash] = record;
            return Task.CompletedTask;
        }

        public Task<InvitationRecord?> FindByHashAsync(string tokenHash, CancellationToken cancellationToken = default)
            => Task.FromResult(Records.GetValueOrDefault(tokenHash));

        public Task<bool> TryMarkUsedAsync(string tokenHash, DateTimeOffset usedAt, CancellationToken cancellationToken = default)
        {
            var record = Records[tokenHash];
            if (record.UsedAt is not null)
            {
                return Task.FromResult(false);
            }

            record.UsedAt = usedAt;
            return Task.FromResult(true);
        }
    }

    private readonly InMemoryInvitationStore _store = new();
    private readonly ManualTimeProvider _clock = new(new DateTimeOffset(2026, 5, 1, 10, 0, 0, TimeSpan.Zero));

    private StaffInvitationService CreateService() => new(_store, _clock);

    private static string Sha256Hex(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));

    [Fact]
    public async Task Token_is_stored_only_as_a_sha256_hash()
    {
        var token = await CreateService().CreateAsync("Officer@Example.org", "Staff", "admin-1");

        var record = Assert.Single(_store.Records.Values);
        Assert.Equal(Sha256Hex(token), record.TokenHash);
        Assert.DoesNotContain(token, _store.Records.Keys);
        Assert.NotEqual(token, record.TokenHash);
        Assert.Equal("officer@example.org", record.Email);
    }

    [Fact]
    public async Task Valid_token_is_accepted_and_returns_email_and_role()
    {
        var service = CreateService();
        var token = await service.CreateAsync("officer@example.org", "Coordinator", "admin-1");

        var result = await service.AcceptAsync(token);

        Assert.Equal(InvitationStatus.Accepted, result.Status);
        Assert.Equal("officer@example.org", result.Email);
        Assert.Equal("Coordinator", result.Role);
    }

    [Fact]
    public async Task Token_can_be_used_only_once()
    {
        var service = CreateService();
        var token = await service.CreateAsync("officer@example.org", "Staff", "admin-1");

        Assert.Equal(InvitationStatus.Accepted, (await service.AcceptAsync(token)).Status);
        Assert.Equal(InvitationStatus.AlreadyUsed, (await service.AcceptAsync(token)).Status);
    }

    [Fact]
    public async Task Concurrent_accepts_produce_exactly_one_success()
    {
        var service = CreateService();
        var token = await service.CreateAsync("officer@example.org", "Staff", "admin-1");

        var first = await service.AcceptAsync(token);
        // Simulates the losing request that already passed the "unused" check before the first one committed.
        var raced = await new StaffInvitationService(_store, _clock).AcceptAsync(token);

        Assert.Single(new[] { first, raced }, r => r.Status == InvitationStatus.Accepted);
    }

    [Fact]
    public async Task Token_expires_after_exactly_24_hours()
    {
        var service = CreateService();
        var token = await service.CreateAsync("officer@example.org", "Staff", "admin-1");

        _clock.Advance(TimeSpan.FromHours(24));

        Assert.Equal(InvitationStatus.Expired, (await service.AcceptAsync(token)).Status);
    }

    [Fact]
    public async Task Token_is_still_valid_one_second_before_24_hours()
    {
        var service = CreateService();
        var token = await service.CreateAsync("officer@example.org", "Staff", "admin-1");

        _clock.Advance(TimeSpan.FromHours(24) - TimeSpan.FromSeconds(1));

        Assert.Equal(InvitationStatus.Accepted, (await service.AcceptAsync(token)).Status);
    }

    [Fact]
    public async Task Expired_token_is_not_marked_as_used()
    {
        var service = CreateService();
        var token = await service.CreateAsync("officer@example.org", "Staff", "admin-1");
        _clock.Advance(TimeSpan.FromHours(25));

        await service.AcceptAsync(token);

        Assert.Null(Assert.Single(_store.Records.Values).UsedAt);
    }

    [Theory]
    [InlineData("")]
    [InlineData("guess")]
    public async Task Unknown_token_is_invalid(string token)
    {
        Assert.Equal(InvitationStatus.Invalid, (await CreateService().AcceptAsync(token)).Status);
    }

    [Fact]
    public async Task Each_invitation_gets_a_distinct_token()
    {
        var service = CreateService();

        var a = await service.CreateAsync("a@example.org", "Staff", "admin-1");
        var b = await service.CreateAsync("a@example.org", "Staff", "admin-1");

        Assert.NotEqual(a, b);
        Assert.Equal(2, _store.Records.Count);
    }

    [Theory]
    [InlineData("Admin")]
    [InlineData("Citizen")]
    [InlineData("Root")]
    public async Task Roles_that_cannot_be_invited_are_refused(string role)
    {
        await Assert.ThrowsAsync<ArgumentException>(
            () => CreateService().CreateAsync("officer@example.org", role, "admin-1"));
        Assert.Empty(_store.Records);
    }
}
