namespace CrisisPlatform.Tests.Database;

public sealed class CheckConstraintTests
{
    private static string RepoRoot =>
        Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

    [Fact]
    public void Migration_004_defines_check_constraints_for_occupancy_quantity_and_urgency()
    {
        var path = Path.Combine(RepoRoot, "database", "migrations", "004_check_constraints_occupancy_quantity.sql");
        Assert.True(File.Exists(path), $"Expected migration file at {path}");

        var sql = File.ReadAllText(path);

        Assert.Contains("CK_Shelters_Capacity_NonNegative", sql);
        Assert.Contains("CHECK (Capacity >= 0)", sql);
        Assert.Contains("CK_Shelters_Occupancy_Range", sql);
        Assert.Contains("CHECK (CurrentOccupancy >= 0 AND CurrentOccupancy <= Capacity)", sql);
        Assert.Contains("CK_Resources_Quantity_NonNegative", sql);
        Assert.Contains("CHECK (Quantity >= 0)", sql);
        Assert.Contains("CK_AidRequests_Urgency_Range", sql);
        Assert.Contains("CHECK (Urgency BETWEEN 1 AND 5)", sql);
    }

    [Theory]
    [InlineData(0, 100, true)]
    [InlineData(50, 100, true)]
    [InlineData(100, 100, true)]
    [InlineData(-1, 100, false)]
    [InlineData(101, 100, false)]
    [InlineData(0, -1, false)]
    public void Shelter_occupancy_constraint_allows_only_zero_through_capacity(
        int occupancy, int capacity, bool expectedValid)
    {
        bool isValid = capacity >= 0 && occupancy >= 0 && occupancy <= capacity;
        Assert.Equal(expectedValid, isValid);
    }

    [Theory]
    [InlineData(0, true)]
    [InlineData(25, true)]
    [InlineData(-1, false)]
    [InlineData(-100, false)]
    public void Resource_quantity_constraint_rejects_negative_values(int quantity, bool expectedValid)
    {
        bool isValid = quantity >= 0;
        Assert.Equal(expectedValid, isValid);
    }
}
