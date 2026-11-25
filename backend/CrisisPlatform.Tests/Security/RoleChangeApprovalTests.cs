using CrisisPlatform.Api.Modules.Admin.RoleChanges;

namespace CrisisPlatform.Tests.Security;

public class RoleChangeApprovalTests
{
    private sealed class InMemoryRoleChangeStore : IRoleChangeStore
    {
        public Dictionary<Guid, RoleChangeRequest> Requests { get; } = new();

        public Task AddAsync(RoleChangeRequest request, CancellationToken cancellationToken = default)
        {
            Requests[request.Id] = request;
            return Task.CompletedTask;
        }

        public Task<RoleChangeRequest?> FindAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(Requests.GetValueOrDefault(id));

        public Task SaveAsync(RoleChangeRequest request, CancellationToken cancellationToken = default)
        {
            Requests[request.Id] = request;
            return Task.CompletedTask;
        }
    }

    private readonly InMemoryRoleChangeStore _store = new();
    private readonly ManualTimeProvider _clock = new(new DateTimeOffset(2026, 6, 1, 9, 0, 0, TimeSpan.Zero));

    private RoleChangeService CreateService() => new(_store, _clock);

    [Fact]
    public async Task Requesting_a_change_does_not_approve_it()
    {
        var id = await CreateService().RequestAsync("user-9", "Coordinator", "admin-1");

        var request = _store.Requests[id];
        Assert.Equal(RoleChangeState.Pending, request.State);
        Assert.Null(request.ApprovedBy);
    }

    [Fact]
    public async Task A_different_admin_can_approve()
    {
        var service = CreateService();
        var id = await service.RequestAsync("user-9", "Coordinator", "admin-1");

        Assert.Equal(RoleChangeDecision.Approved, await service.ApproveAsync(id, "admin-2"));

        var request = _store.Requests[id];
        Assert.Equal(RoleChangeState.Approved, request.State);
        Assert.Equal("admin-2", request.ApprovedBy);
        Assert.Equal(_clock.GetUtcNow(), request.ApprovedAt);
    }

    [Fact]
    public async Task The_requester_cannot_approve_their_own_request()
    {
        var service = CreateService();
        var id = await service.RequestAsync("user-9", "Admin", "admin-1");

        Assert.Equal(RoleChangeDecision.SameAsRequester, await service.ApproveAsync(id, "admin-1"));

        Assert.Equal(RoleChangeState.Pending, _store.Requests[id].State);
    }

    [Fact]
    public async Task The_person_receiving_the_role_cannot_approve_it()
    {
        var service = CreateService();
        var id = await service.RequestAsync("admin-2", "Admin", "admin-1");

        Assert.Equal(RoleChangeDecision.SameAsTarget, await service.ApproveAsync(id, "admin-2"));

        Assert.Equal(RoleChangeState.Pending, _store.Requests[id].State);
    }

    [Fact]
    public async Task Approver_comparison_is_exact_so_case_variants_are_different_people()
    {
        var service = CreateService();
        var id = await service.RequestAsync("user-9", "Staff", "admin-1");

        Assert.Equal(RoleChangeDecision.Approved, await service.ApproveAsync(id, "ADMIN-1"));
    }

    [Fact]
    public async Task A_request_cannot_be_approved_twice()
    {
        var service = CreateService();
        var id = await service.RequestAsync("user-9", "Staff", "admin-1");
        await service.ApproveAsync(id, "admin-2");

        Assert.Equal(RoleChangeDecision.AlreadyApproved, await service.ApproveAsync(id, "admin-3"));
        Assert.Equal("admin-2", _store.Requests[id].ApprovedBy);
    }

    [Fact]
    public async Task A_stale_request_expires_after_72_hours()
    {
        var service = CreateService();
        var id = await service.RequestAsync("user-9", "Staff", "admin-1");

        _clock.Advance(TimeSpan.FromHours(72));

        Assert.Equal(RoleChangeDecision.Expired, await service.ApproveAsync(id, "admin-2"));
        Assert.Equal(RoleChangeState.Pending, _store.Requests[id].State);
    }

    [Fact]
    public async Task Unknown_request_is_not_found()
    {
        Assert.Equal(RoleChangeDecision.NotFound, await CreateService().ApproveAsync(Guid.NewGuid(), "admin-2"));
    }

    [Theory]
    [InlineData("SuperUser")]
    [InlineData("")]
    [InlineData("admin")]
    public async Task Unknown_roles_cannot_be_requested(string role)
    {
        await Assert.ThrowsAsync<ArgumentException>(() => CreateService().RequestAsync("user-9", role, "admin-1"));
        Assert.Empty(_store.Requests);
    }
}
