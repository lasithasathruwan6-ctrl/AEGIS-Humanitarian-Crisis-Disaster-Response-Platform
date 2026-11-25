using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;

namespace CrisisPlatform.Tests.Security;

/// <summary>Builds a minimal in-memory host so a single security component can be tested in isolation.</summary>
internal static class TestHostFactory
{
    public static async Task<HttpClient> CreateClientAsync(
        Action<WebApplicationBuilder>? configureServices,
        Action<WebApplication> configurePipeline)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        configureServices?.Invoke(builder);

        var app = builder.Build();
        configurePipeline(app);
        await app.StartAsync();
        return app.GetTestClient();
    }
}
