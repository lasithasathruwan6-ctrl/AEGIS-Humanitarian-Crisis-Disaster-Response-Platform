namespace CrisisPlatform.Tests.Database;

public sealed class ForeignKeyDeleteBehaviourTests
{
    private static string RepoRoot =>
        Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

    [Fact]
    public void Migration_007_uses_no_action_for_historical_and_cross_aggregate_foreign_keys()
    {
        var path = Path.Combine(RepoRoot, "database", "migrations", "007_foreign_key_delete_behaviours.sql");
        Assert.True(File.Exists(path), $"Expected migration file at {path}");

        var sql = File.ReadAllText(path);

        Assert.Contains("FK_AidRequests_Users_CitizenId", sql);
        Assert.Contains("FK_Donations_Users_DonorId", sql);
        Assert.Contains("FK_AiRecommendations_AidRequests_RequestId", sql);
        Assert.Contains("FK_RoleChangeRequests_Users_TargetUserId", sql);
        Assert.Contains("ON DELETE NO ACTION", sql);

        // Ephemeral auth tokens may cascade
        Assert.Contains("FK_RefreshTokens_Users_UserId", sql);
        Assert.Contains("FK_OtpChallenges_Users_UserId", sql);
    }
}
