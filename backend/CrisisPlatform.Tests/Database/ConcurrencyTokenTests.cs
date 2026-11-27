namespace CrisisPlatform.Tests.Database;

public sealed class ConcurrencyTokenTests
{
    private sealed class ConcurrencyCheckedRecord
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public string Status { get; private set; } = "Pending";
        public long RowVersion { get; private set; } = 1;

        public void UpdateStatus(string newStatus, long expectedRowVersion)
        {
            if (expectedRowVersion != RowVersion)
            {
                throw new InvalidOperationException(
                    $"Concurrency conflict on {Id}: expected RowVersion {expectedRowVersion}, actual {RowVersion}.");
            }

            Status = newStatus;
            RowVersion++;
        }
    }

    [Fact]
    public void Update_with_current_concurrency_token_succeeds_and_advances_version()
    {
        var record = new ConcurrencyCheckedRecord();
        long initialVersion = record.RowVersion;

        record.UpdateStatus("Approved", initialVersion);

        Assert.Equal("Approved", record.Status);
        Assert.Equal(initialVersion + 1, record.RowVersion);
    }

    [Fact]
    public void Update_with_stale_concurrency_token_throws_conflict()
    {
        var record = new ConcurrencyCheckedRecord();
        long staleVersion = record.RowVersion;

        // First writer commits and advances RowVersion
        record.UpdateStatus("InProgress", staleVersion);

        // Second writer attempts update with stale RowVersion
        var ex = Assert.Throws<InvalidOperationException>(() =>
            record.UpdateStatus("Completed", staleVersion));

        Assert.Contains("Concurrency conflict", ex.Message);
        Assert.Equal("InProgress", record.Status);
        Assert.Equal(staleVersion + 1, record.RowVersion);
    }
}
