using System.Net;
using System.Text.Json;
using CrisisPlatform.Api.Common.OpenApi;
using CrisisPlatform.Api.Modules.Requests;
using CrisisPlatform.Tests.Security;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace CrisisPlatform.Tests.OpenApi;

/// <summary>Verifies the generated OpenAPI specification includes JWT auth, examples, and error codes.</summary>
public sealed class OpenApiSpecTests
{
    private static Task<HttpClient> CreateClientAsync() =>
        TestHostFactory.CreateClientAsync(
            builder =>
            {
                builder.Services.AddControllers()
                    .AddApplicationPart(typeof(RequestsController).Assembly);
                builder.Services.AddCrisisOpenApi();
                builder.Services.AddSingleton(TimeProvider.System);
                builder.Services.AddScoped<RequestService>();
            },
            app =>
            {
                app.MapOpenApi();
                app.UseRouting();
                app.MapControllers();
            });

    [Fact]
    public async Task OpenApiDocument_IncludesJwtBearerScheme_AndErrorCodes()
    {
        using var client = await CreateClientAsync();
        var response = await client.GetAsync("/openapi/v1.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);

        var root = doc.RootElement;
        var schemes = root.GetProperty("components").GetProperty("securitySchemes");
        Assert.True(schemes.TryGetProperty(OpenApiExtensions.BearerSchemeId, out var bearer));
        Assert.Equal("http", bearer.GetProperty("type").GetString());
        Assert.Equal("bearer", bearer.GetProperty("scheme").GetString());

        var postResponses = root
            .GetProperty("paths")
            .GetProperty("/api/v1/requests")
            .GetProperty("post")
            .GetProperty("responses");

        Assert.True(postResponses.TryGetProperty("400", out _));
        Assert.True(postResponses.TryGetProperty("401", out _));
        Assert.True(postResponses.TryGetProperty("403", out _));
        Assert.True(postResponses.TryGetProperty("409", out _));
    }
}
