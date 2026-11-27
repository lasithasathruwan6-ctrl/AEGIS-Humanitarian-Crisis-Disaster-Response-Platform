using CrisisPlatform.Api.Common.Security.Nic;

namespace CrisisPlatform.Tests.Database;

public sealed class UniqueIndexMigrationTests
{
    private static string RepoRoot =>
        Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

    [Fact]
    public void Migration_003_defines_unique_indexes_for_nic_hash_and_idempotency_keys()
    {
        var path = Path.Combine(RepoRoot, "database", "migrations", "003_unique_indexes_nic_and_idempotency.sql");
        Assert.True(File.Exists(path), $"Expected migration file at {path}");

        var sql = File.ReadAllText(path);

        Assert.Contains("CREATE UNIQUE NONCLUSTERED INDEX UX_Users_NicHash", sql);
        Assert.Contains("ON dbo.Users (NicHash ASC)", sql);
        Assert.Contains("CREATE UNIQUE NONCLUSTERED INDEX UX_AidRequests_IdempotencyKey", sql);
        Assert.Contains("ON dbo.AidRequests (IdempotencyKey ASC)", sql);
        Assert.Contains("CREATE UNIQUE NONCLUSTERED INDEX UX_OutboxMessages_IdempotencyKey", sql);
        Assert.Contains("ON dbo.OutboxMessages (IdempotencyKey ASC)", sql);
        Assert.Contains("IF NOT EXISTS", sql);
    }

    [Fact]
    public void NicHasher_produces_fixed_length_deterministic_key_for_unique_index()
    {
        var key = Enumerable.Range(1, 32).Select(i => (byte)i).ToArray();
        var hasher = new NicHasher(key);

        var oldHash = hasher.Hash("901234567V");
        var newHash = hasher.Hash("199012304567");

        Assert.Equal(64, oldHash.Length);
        Assert.Equal(oldHash, newHash);
    }
}
