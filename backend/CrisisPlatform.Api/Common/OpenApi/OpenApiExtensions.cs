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
            options.AddOperationTransformer<StandardErrorResponsesOperationTransformer>();
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

    private sealed class StandardErrorResponsesOperationTransformer : IOpenApiOperationTransformer
    {
        private static readonly (string Code, string Description)[] StandardErrors =
        [
            ("400", "Bad Request — validation failed or required header missing."),
            ("401", "Unauthorized — missing or invalid JWT bearer token."),
            ("403", "Forbidden — authenticated user lacks the required role or scope."),
            ("409", "Conflict — idempotency key reused with a different payload or state conflict."),
        ];

        public Task TransformAsync(
            OpenApiOperation operation,
            OpenApiOperationTransformerContext context,
            CancellationToken cancellationToken)
        {
            operation.Responses ??= new OpenApiResponses();

            foreach (var (code, description) in StandardErrors)
            {
                if (!operation.Responses.ContainsKey(code))
                {
                    operation.Responses[code] = new OpenApiResponse
                    {
                        Description = description,
                    };
                }
            }

            return Task.CompletedTask;
        }
    }
}
