using CrisisPlatform.Api.Common.Security;
using Microsoft.Extensions.DependencyInjection;

namespace CrisisPlatform.Tests.Security;

public class CorsTests
{
    private const string Frontend = "https://app.example.org";

    private static Task<HttpClient> CreateClient(string? origin = Frontend) => TestHostFactory.CreateClientAsync(
        builder =>
        {
            builder.Configuration[CorsExtensions.AllowedOriginKey] = origin;
            builder.Services.AddFrontendCors(builder.Configuration);
        },
        app =>
        {
            app.UseCors(CorsExtensions.PolicyName);
            app.MapGet("/ok", () => "ok");
        });

    private static HttpRequestMessage Get(string origin)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/ok");
        request.Headers.Add("Origin", origin);
        return request;
    }

    private static HttpRequestMessage Preflight(string origin)
    {
        var request = new HttpRequestMessage(HttpMethod.Options, "/ok");
        request.Headers.Add("Origin", origin);
        request.Headers.Add("Access-Control-Request-Method", "POST");
        return request;
    }

    [Fact]
    public async Task Configured_frontend_origin_is_allowed()
    {
        var client = await CreateClient();

        var response = await client.SendAsync(Get(Frontend));

        Assert.Equal(Frontend, Assert.Single(response.Headers.GetValues("Access-Control-Allow-Origin")));
    }

    [Theory]
    [InlineData("https://evil.example.com")]
    [InlineData("http://app.example.org")]
    [InlineData("https://app.example.org.evil.com")]
    public async Task Other_origins_receive_no_cors_headers(string origin)
    {
        var client = await CreateClient();

        var response = await client.SendAsync(Get(origin));

        Assert.False(response.Headers.Contains("Access-Control-Allow-Origin"));
    }

    [Fact]
    public async Task Preflight_from_foreign_origin_is_not_approved()
    {
        var client = await CreateClient();

        var response = await client.SendAsync(Preflight("https://evil.example.com"));

        Assert.False(response.Headers.Contains("Access-Control-Allow-Origin"));
    }

    [Fact]
    public async Task Preflight_from_frontend_is_approved_without_wildcards()
    {
        var client = await CreateClient();

        var response = await client.SendAsync(Preflight(Frontend));

        Assert.Equal(Frontend, Assert.Single(response.Headers.GetValues("Access-Control-Allow-Origin")));
        Assert.DoesNotContain("*", response.Headers.GetValues("Access-Control-Allow-Headers"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("*")]
    [InlineData("https://*.example.org")]
    [InlineData("app.example.org")]
    [InlineData("https://app.example.org/some/path")]
    public async Task Startup_fails_when_origin_is_missing_or_unsafe(string? origin)
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() => CreateClient(origin));
    }
}
