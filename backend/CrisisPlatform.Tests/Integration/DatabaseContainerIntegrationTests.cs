using System.Net.Sockets;

namespace CrisisPlatform.Tests.Integration;

/// <summary>
/// Runs database container integration tests only when a SQL Server 2022 container
/// is configured via <c>TEST_SQLSERVER_CONNECTION_STRING</c>; otherwise skips with a clear reason.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class SqlServerContainerFactAttribute : FactAttribute
{
    public SqlServerContainerFactAttribute()
    {
        var connStr = Environment.GetEnvironmentVariable("TEST_SQLSERVER_CONNECTION_STRING");
        if (string.IsNullOrWhiteSpace(connStr))
        {
            Skip = "SQL Server 2022 container unavailable: set TEST_SQLSERVER_CONNECTION_STRING to run.";
        }
    }
}

public sealed class DatabaseContainerIntegrationTests
{
    private static string RepoRoot =>
        Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

    [SqlServerContainerFact]
    public async Task SqlServer_container_is_reachable_and_migrations_exist()
    {
        var connStr = Environment.GetEnvironmentVariable("TEST_SQLSERVER_CONNECTION_STRING")!;
        Assert.False(string.IsNullOrWhiteSpace(connStr));

        // Verify TCP connectivity to the configured SQL Server container port (default 1433)
        var hostPart = connStr.Split(';', StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault(p => p.StartsWith("Server=", StringComparison.OrdinalIgnoreCase))?
            .Split('=', 2)[1] ?? "localhost,1433";

        var segments = hostPart.Split(',');
        var host = segments[0];
        var port = segments.Length > 1 && int.TryParse(segments[1], out var p) ? p : 1433;

        using var tcp = new TcpClient();
        await tcp.ConnectAsync(host, port);
        Assert.True(tcp.Connected);

        var migrationsDir = Path.Combine(RepoRoot, "database", "migrations");
        Assert.NotEmpty(Directory.GetFiles(migrationsDir, "*.sql"));
    }
}
