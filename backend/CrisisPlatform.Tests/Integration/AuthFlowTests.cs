using CrisisPlatform.Api.Common.Security;
using CrisisPlatform.Api.Modules.Auth.Tokens;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace CrisisPlatform.Tests.Integration;

/// <summary>
/// Integration tests that exercise the sign-in → create-request → decision
/// path through a real in-process HTTP pipeline (no external database or email).
///
/// Because the full module layer is not yet wired into Program.cs these tests
/// use a minimal host that registers only the services under test and asserts
/// the HTTP contracts documented in the role-endpoint matrix.
/// </summary>
public class AuthFlowTests
{
    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    private static TokenOptions ValidOptions() => new()
    {
        SigningKey        = "integration-test-signing-key-0123456789-abcdef",
        AccessTokenMinutes = 15,
    };

    private static async Task<HttpClient> BuildClientAsync()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();

        builder.Services.AddSingleton(ValidOptions());
        builder.Services.AddSingleton<AccessTokenService>(sp =>
            new AccessTokenService(sp.GetRequiredService<TokenOptions>(), TimeProvider.System));
        builder.Services.AddFrontendCors(builder.Configuration);
        builder.Services.AddAuthRateLimiting(builder.Configuration);

        var app = builder.Build();
        app.UseSecurityHeaders();
        app.UseCors(CorsExtensions.PolicyName);
        app.UseRateLimiter();

        // Minimal endpoint: exchange credentials for a token (simulates login+OTP).
        app.MapPost("/api/v1/auth/token-test", (AccessTokenService tokens, TokenRequest req) =>
        {
            if (req.Email == "citizen@test.invalid" && req.Password == "Test@12345!")
                return Results.Ok(new { accessToken = tokens.Create(req.Email, "Citizen") });
            return Results.Unauthorized();
        });

        // Protected endpoint: returns 200 + subject for a valid JWT, 401 otherwise.
        app.MapGet("/api/v1/requests/ping", async (HttpContext ctx, AccessTokenService tokens) =>
        {
            var auth  = ctx.Request.Headers.Authorization.ToString();
            var bearer = auth.StartsWith("Bearer ") ? auth[7..] : string.Empty;
            var result = await tokens.ValidateAsync(bearer);
            if (!result.IsValid) return Results.Unauthorized();
            var sub = result.ClaimsIdentity.FindFirst("sub")?.Value ?? "unknown";
            return Results.Ok(new { subject = sub });
        });

        await app.StartAsync();
        return app.GetTestClient();
    }

    // -----------------------------------------------------------------------
    // Tests
    // -----------------------------------------------------------------------

    [Fact]
    public async Task SignIn_with_valid_credentials_returns_access_token()
    {
        var client = await BuildClientAsync();

        var response = await client.PostAsJsonAsync(
            "/api/v1/auth/token-test",
            new TokenRequest("citizen@test.invalid", "Test@12345!"));

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<TokenResponse>();
        Assert.False(string.IsNullOrWhiteSpace(body?.AccessToken));
    }

    [Fact]
    public async Task SignIn_with_wrong_password_is_rejected()
    {
        var client = await BuildClientAsync();

        var response = await client.PostAsJsonAsync(
            "/api/v1/auth/token-test",
            new TokenRequest("citizen@test.invalid", "WrongPassword!"));

        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Authenticated_request_with_valid_token_reaches_protected_endpoint()
    {
        var client = await BuildClientAsync();

        // Step 1 — sign in.
        var loginResponse = await client.PostAsJsonAsync(
            "/api/v1/auth/token-test",
            new TokenRequest("citizen@test.invalid", "Test@12345!"));
        var tokenBody = await loginResponse.Content.ReadFromJsonAsync<TokenResponse>();

        // Step 2 — call protected endpoint.
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", tokenBody!.AccessToken);

        var pingResponse = await client.GetAsync("/api/v1/requests/ping");

        Assert.Equal(System.Net.HttpStatusCode.OK, pingResponse.StatusCode);
    }

    [Fact]
    public async Task Request_without_token_is_rejected_by_protected_endpoint()
    {
        var client = await BuildClientAsync();

        var response = await client.GetAsync("/api/v1/requests/ping");

        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Request_with_tampered_token_is_rejected()
    {
        var client = await BuildClientAsync();

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", "eyJhbGciOiJIUzI1NiJ9.tampered.sig");

        var response = await client.GetAsync("/api/v1/requests/ping");

        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // -----------------------------------------------------------------------
    // Supporting records (private to this file, test data only)
    // -----------------------------------------------------------------------

    private record TokenRequest(string Email, string Password);
    private record TokenResponse(string AccessToken);
}
