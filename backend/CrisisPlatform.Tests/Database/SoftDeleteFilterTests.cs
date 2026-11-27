namespace CrisisPlatform.Tests.Database;

public sealed class SoftDeleteFilterTests
{
    private static string RepoRoot =>
        Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

    private sealed record SoftDeletableEntity(Guid Id, string NicHash, DateTimeOffset? DeletedAt);

    private static IReadOnlyList<SoftDeletableEntity> QueryActive(IEnumerable<SoftDeletableEntity> source)
        => source.Where(e => e.DeletedAt is null).ToList();

    private static IReadOnlyList<SoftDeletableEntity> QueryAll(IEnumerable<SoftDeletableEntity> source)
        => source.ToList();

    [Fact]
    public void Global_soft_delete_filter_excludes_deleted_rows_by_default()
    {
        var active = new SoftDeletableEntity(Guid.NewGuid(), "hash-1", null);
        var deleted = new SoftDeletableEntity(Guid.NewGuid(), "hash-2", DateTimeOffset.UtcNow);

        var results = QueryActive([active, deleted]);

        Assert.Single(results);
        Assert.Equal(active.Id, results[0].Id);
    }

    [Fact]
    public void Explicit_ignore_filter_returns_both_active_and_soft_deleted_rows()
    {
        var active = new SoftDeletableEntity(Guid.NewGuid(), "hash-1", null);
        var deleted = new SoftDeletableEntity(Guid.NewGuid(), "hash-2", DateTimeOffset.UtcNow);

        var results = QueryAll([active, deleted]);

        Assert.Equal(2, results.Count);
    }

    [Fact]
    public void Filtered_indexes_exclude_soft_deleted_rows()
    {
        var m001 = File.ReadAllText(Path.Combine(RepoRoot, "database", "migrations", "001_requests_status_district_created_indexes.sql"));
        var m003 = File.ReadAllText(Path.Combine(RepoRoot, "database", "migrations", "003_unique_indexes_nic_and_idempotency.sql"));

        Assert.Contains("WHERE DeletedAt IS NULL", m001);
        Assert.Contains("WHERE NicHash IS NOT NULL AND DeletedAt IS NULL", m003);
        Assert.Contains("WHERE IdempotencyKey IS NOT NULL AND DeletedAt IS NULL", m003);
    }
}
