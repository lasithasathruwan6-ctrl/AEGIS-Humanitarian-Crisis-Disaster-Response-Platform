namespace CrisisPlatform.Tests.Database;

public sealed class OutboxCleanupTests
{
    private static string RepoRoot =>
        Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

    [Fact]
    public void Cleanup_script_deletes_only_old_delivered_rows_in_batches()
    {
        var path = Path.Combine(RepoRoot, "database", "scripts", "cleanup-old-outbox.sql");
        Assert.True(File.Exists(path), $"Expected cleanup script at {path}");

        var sql = File.ReadAllText(path);

        Assert.Contains("DELETE TOP (@BatchSize)", sql);
        Assert.Contains("WHERE Status = N'Delivered'", sql);
        Assert.Contains("SentAt IS NOT NULL", sql);
        Assert.Contains("SentAt < @Cutoff", sql);
        Assert.DoesNotContain("Pending", sql);
        Assert.DoesNotContain("DeadLettered", sql);
    }
}
