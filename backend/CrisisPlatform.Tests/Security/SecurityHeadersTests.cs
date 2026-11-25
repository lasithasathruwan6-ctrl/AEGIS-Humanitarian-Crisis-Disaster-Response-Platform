using CrisisPlatform.Api.Common.Security;

namespace CrisisPlatform.Tests.Security;

public class SecurityHeadersTests
{
    private static Task<HttpClient> CreateClient() => TestHostFactory.CreateClientAsync(null, app =>
    {
        app.UseSecurityHeaders();
        app.MapGet("/ok", () => "ok");
    });

    [Theory]
    [InlineData("X-Content-Type-Options", "nosniff")]
    [InlineData("X-Frame-Options", "DENY")]
    [InlineData("Referrer-Policy", "no-referrer")]
    [InlineData("Cross-Origin-Resource-Policy", "same-site")]
    public async Task Successful_responses_carry_security_headers(string name, string expected)
    {
        var client = await CreateClient();

        var response = await client.GetAsync("/ok");

        Assert.Equal(expected, Assert.Single(response.Headers.GetValues(name)));
    }

    [Fact]
    public async Task Content_security_policy_forbids_framing_and_default_sources()
    {
        var client = await CreateClient();

        var response = await client.GetAsync("/ok");

        var csp = Assert.Single(response.Headers.GetValues("Content-Security-Policy"));
        Assert.Contains("default-src 'none'", csp);
        Assert.Contains("frame-ancestors 'none'", csp);
    }

    [Fact]
    public async Task Error_responses_also_carry_security_headers()
    {
        var client = await CreateClient();

        var response = await client.GetAsync("/missing");

        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        Assert.True(response.Headers.Contains("X-Content-Type-Options"));
    }
}
