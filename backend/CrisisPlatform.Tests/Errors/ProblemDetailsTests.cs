using System.Net;
using System.Text.Json;
using CrisisPlatform.Api.Common.Errors;
using CrisisPlatform.Tests.Security;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CrisisPlatform.Tests.Errors;

/// <summary>Integration tests verifying consistent RFC 9457 Problem Details responses.</summary>
public sealed class ProblemDetailsTests
{
    private static Task<HttpClient> CreateClientAsync() =>
        TestHostFactory.CreateClientAsync(
            builder => builder.Services.AddApiProblemDetails(),
            app =>
            {
                app.UseExceptionHandler();
                app.MapGet("/throw/arg",      () => { throw new ArgumentException("Invalid district."); });
                app.MapGet("/throw/notfound", () => { throw new KeyNotFoundException("Request missing."); });
                app.MapGet("/throw/conflict", () => { throw new IdempotencyConflictException("Key reused."); });
                app.MapGet("/throw/unauth",   () => { throw new UnauthorizedAccessException(); });
                app.MapGet("/throw/boom",     () => { throw new Exception("Sensitive DB connection string!"); });
            });

    [Theory]
    [InlineData("/throw/arg",      HttpStatusCode.BadRequest,          "Validation error")]
    [InlineData("/throw/notfound", HttpStatusCode.NotFound,            "Resource not found")]
    [InlineData("/throw/conflict", HttpStatusCode.Conflict,            "Idempotency conflict")]
    [InlineData("/throw/unauth",   HttpStatusCode.Unauthorized,        "Unauthorized")]
    [InlineData("/throw/boom",     HttpStatusCode.InternalServerError, "Internal server error")]
    public async Task Exceptions_AreMappedTo_ConsistentProblemDetails(
        string path,
        HttpStatusCode expectedStatus,
        string expectedTitle)
    {
        using var client = await CreateClientAsync();
        var response = await client.GetAsync(path);

        Assert.Equal(expectedStatus, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var body = await response.Content.ReadAsStringAsync();
        var problem = JsonSerializer.Deserialize<ProblemDetails>(body, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(problem);
        Assert.Equal((int)expectedStatus, problem.Status);
        Assert.Equal(expectedTitle, problem.Title);
        Assert.Equal(path, problem.Instance);
        Assert.True(problem.Extensions.ContainsKey("traceId"));
    }

    [Fact]
    public async Task UnhandledException_DoesNotLeakInternalMessage()
    {
        using var client = await CreateClientAsync();
        var response = await client.GetAsync("/throw/boom");
        var body = await response.Content.ReadAsStringAsync();

        Assert.DoesNotContain("Sensitive DB connection string", body);
        Assert.Contains("An unexpected error occurred.", body);
    }
}
