namespace CrisisPlatform.Tests.Database;

public sealed class AuditTableImmutabilityTests
{
    private static string RepoRoot =>
        Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

    [Fact]
    public void Audit_immutability_verification_script_tests_update_delete_and_truncate()
    {
        var path = Path.Combine(RepoRoot, "database", "scripts", "verify-audit-immutability.sql");
        Assert.True(File.Exists(path), $"Expected verification script at {path}");

        var sql = File.ReadAllText(path);

        Assert.Contains("UPDATE", sql);
        Assert.Contains("DELETE", sql);
        Assert.Contains("TRUNCATE TABLE dbo.AuditEntries", sql);
        Assert.Contains("BEGIN TRY", sql);
        Assert.Contains("BEGIN CATCH", sql);
    }
}
