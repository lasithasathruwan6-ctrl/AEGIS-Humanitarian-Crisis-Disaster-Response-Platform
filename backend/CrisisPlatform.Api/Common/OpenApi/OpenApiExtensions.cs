using System.Text.Json.Nodes;
using CrisisPlatform.Api.Modules.Requests;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace CrisisPlatform.Api.Common.OpenApi;

/// <summary>
/// Configures OpenAPI / Swagger documentation with JWT Bearer authentication
/// and concrete request-body examples.
/// </summary>
public static class OpenApiExtensions
{
    public const string BearerSchemeId = "Bearer";

    public static IServiceCollection AddCrisisOpenApi(this IServiceCollection services)
    {
        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer<JwtBearerDocumentTransformer>();
            options.AddSchemaTransformer<RequestExampleSchemaTransformer>();
        });

        return services;
    }

    private sealed class JwtBearerDocumentTransformer : IOpenApiDocumentTransformer
    {
        public Task TransformAsync(
            OpenApiDocument document,
            OpenApiDocumentTransformerContext context,
            CancellationToken cancellationToken)
        {
            document.Info.Title   = "Crisis Aid Platform API";
            document.Info.Version = "v1";
            document.Info.Description =
                "ASP.NET Core Modular-Monolith API for coordinating emergency aid, shelters, volunteers, and resources.";

            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
            document.Components.SecuritySchemes[BearerSchemeId] = new OpenApiSecurityScheme
            {
                Type         = SecuritySchemeType.Http,
                Scheme       = "bearer",
                BearerFormat = "JWT",
                Description  = "Paste a valid JWT access token obtained from /api/v1/auth/login.",
            };

            document.Security ??= [];
            document.Security.Add(new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference(BearerSchemeId, document)] = []
            });

            return Task.CompletedTask;
        }
    }

    private sealed class RequestExampleSchemaTransformer : IOpenApiSchemaTransformer
    {
        public Task TransformAsync(
            OpenApiSchema schema,
            OpenApiSchemaTransformerContext context,
            CancellationToken cancellationToken)
        {
            if (context.JsonTypeInfo.Type == typeof(CreateRequestDto))
            {
                schema.Examples =
                [
                    new JsonObject
                    {
                        ["nicHash"]      = "a1b2c3d4e5f67890abcdef1234567890",
                        ["district"]     = "Colombo",
                        ["needType"]     = "Water",
                        ["urgency"]      = 4,
                        ["description"]  = "Clean drinking water needed for 15 displaced residents.",
                        ["contactPhone"] = "0771234567",
                    }
                ];
            }

            return Task.CompletedTask;
        }
    }
}
