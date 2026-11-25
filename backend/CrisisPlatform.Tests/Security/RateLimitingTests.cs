using System.Net;
using CrisisPlatform.Api.Common.Security;

namespace CrisisPlatform.Tests.Security;

public class RateLimitingTests
{
    private static Task<HttpClient> CreateClient(int permitLimit = 3) => TestHostFactory.CreateClientAsync(
        builder =>
        {
            builder.Configuration[$"{AuthRateLimitOptions.SectionName}:PermitLimit"] = permitLimit.ToString();
            builder.Configuration[$"{AuthRateLimitOptions.SectionName}:WindowSeconds"] = "60";
            builder.Services.AddAuthRateLimiting(builder.Configuration);
        },
        app =>
        {
            app.UseRateLimiter();
            app.MapPost("/api/v1/auth/login", () => "login");
            app.MapPost("/api/v1/auth/register", () => "register");
            app.MapGet("/api/v1/crises", () => "crises");
        });

    private static async Task<HttpStatusCode> Post(HttpClient client, string path)
        => (await client.PostAsync(path, new StringContent("{}"))).StatusCode;

    [Fact]
    public async Task Login_is_throttled_after_the_permit_limit()
    {
        var client = await CreateClient(permitLimit: 3);

        for (var i = 0; i < 3; i++)
        {
            Assert.Equal(HttpStatusCode.OK, await Post(client, "/api/v1/auth/login"));
        }

        var blocked = await client.PostAsync("/api/v1/auth/login", new StringContent("{}"));

        Assert.Equal(HttpStatusCode.TooManyRequests, blocked.StatusCode);
        Assert.True(blocked.Headers.Contains("Retry-After"));
    }

    [Fact]
    public async Task Registration_is_throttled_after_the_permit_limit()
    {
        var client = await CreateClient(permitLimit: 2);

        Assert.Equal(HttpStatusCode.OK, await Post(client, "/api/v1/auth/register"));
        Assert.Equal(HttpStatusCode.OK, await Post(client, "/api/v1/auth/register"));
        Assert.Equal(HttpStatusCode.TooManyRequests, await Post(client, "/api/v1/auth/register"));
    }

    [Fact]
    public async Task Login_and_registration_have_independent_counters()
    {
        var client = await CreateClient(permitLimit: 1);

        Assert.Equal(HttpStatusCode.OK, await Post(client, "/api/v1/auth/login"));
        Assert.Equal(HttpStatusCode.TooManyRequests, await Post(client, "/api/v1/auth/login"));
        Assert.Equal(HttpStatusCode.OK, await Post(client, "/api/v1/auth/register"));
    }

    [Fact]
    public async Task Other_endpoints_are_not_throttled()
    {
        var client = await CreateClient(permitLimit: 1);

        for (var i = 0; i < 10; i++)
        {
            Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/v1/crises")).StatusCode);
        }
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-1")]
    public async Task Startup_fails_on_non_positive_limits(string permit)
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() => TestHostFactory.CreateClientAsync(
            builder =>
            {
                builder.Configuration[$"{AuthRateLimitOptions.SectionName}:PermitLimit"] = permit;
                builder.Services.AddAuthRateLimiting(builder.Configuration);
            },
            _ => { }));
    }
}
