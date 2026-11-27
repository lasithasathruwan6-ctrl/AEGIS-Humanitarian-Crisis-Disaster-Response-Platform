namespace CrisisPlatform.Tests.Database;

public sealed class AuditTriggerMigrationTests
{
    private static string RepoRoot =>
        Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

    [Fact]
    public void Migration_006_creates_instead_of_update_and_delete_triggers_for_audit_entries()
    {
        var path = Path.Combine(RepoRoot, "database", "migrations", "006_audit_instead_of_triggers.sql");
        Assert.True(File.Exists(path), $"Expected migration file at {path}");

        var sql = File.ReadAllText(path);

        Assert.Contains("TR_AuditEntries_BlockUpdate", sql);
        Assert.Contains("INSTEAD OF UPDATE", sql);
        Assert.Contains("TR_AuditEntries_BlockDelete", sql);
        Assert.Contains("INSTEAD OF DELETE", sql);
        Assert.Contains("THROW 51000", sql);
        Assert.Contains("THROW 51001", sql);
    }
}
