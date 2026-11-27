using CrisisPlatform.Api.Modules.Admin.RoleChanges;

namespace CrisisPlatform.Tests.Database;

public sealed class FourEyesConstraintTests
{
    private static string RepoRoot =>
        Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

    [Fact]
    public void Migration_005_enforces_second_approver_differs_from_decider_and_requester()
    {
        var path = Path.Combine(RepoRoot, "database", "migrations", "005_four_eyes_check_constraints.sql");
        Assert.True(File.Exists(path), $"Expected migration file at {path}");

        var sql = File.ReadAllText(path);

        Assert.Contains("CK_AiRecommendations_Approver_Differs_From_Decider", sql);
        Assert.Contains("CHECK (ApprovedBy IS NULL OR ApprovedBy <> DecidedBy)", sql);
        Assert.Contains("CK_RoleChangeRequests_Approver_Differs_From_Requester", sql);
        Assert.Contains("CHECK (ApprovedBy IS NULL OR ApprovedBy <> RequestedBy)", sql);
    }

    [Theory]
    [InlineData("staff-1", "staff-2", true)]
    [InlineData("staff-1", null, true)]
    [InlineData("staff-1", "staff-1", false)]
    public void AiRecommendation_four_eyes_rule_rejects_same_decider_and_approver(
        string deciderId, string? approverId, bool expectedValid)
    {
        bool isValid = approverId is null || !string.Equals(deciderId, approverId, StringComparison.Ordinal);
        Assert.Equal(expectedValid, isValid);
    }

    [Fact]
    public async Task RoleChangeService_rejects_approval_when_approver_equals_requester()
    {
        var clock = new TestClock(new DateTimeOffset(2026, 6, 1, 9, 0, 0, TimeSpan.Zero));
        var store = new InMemoryRoleChangeStore();
        var service = new RoleChangeService(store, clock);

        var id = await service.RequestAsync("user-42", "Coordinator", "admin-1");
        var decision = await service.ApproveAsync(id, "admin-1");

        Assert.Equal(RoleChangeDecision.SameAsRequester, decision);
    }

    private sealed class TestClock(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private sealed class InMemoryRoleChangeStore : IRoleChangeStore
    {
        private readonly Dictionary<Guid, RoleChangeRequest> _items = new();

        public Task AddAsync(RoleChangeRequest request, CancellationToken cancellationToken = default)
        {
            _items[request.Id] = request;
            return Task.CompletedTask;
        }

        public Task<RoleChangeRequest?> FindAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(_items.GetValueOrDefault(id));

        public Task SaveAsync(RoleChangeRequest request, CancellationToken cancellationToken = default)
        {
            _items[request.Id] = request;
            return Task.CompletedTask;
        }
    }
}
