using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace CrisisPlatform.Api.Common.Errors;

/// <summary>Thrown when an <c>Idempotency-Key</c> is reused with a conflicting payload.</summary>
public sealed class IdempotencyConflictException(string message) : Exception(message);

/// <summary>
/// Maps unhandled exceptions to consistent RFC 9457 Problem Details responses
/// (<c>application/problem+json</c>) without leaking stack traces.
/// </summary>
public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title, detail) = exception switch
        {
            ArgumentOutOfRangeException ex => (StatusCodes.Status400BadRequest, "Validation error", ex.Message),
            ArgumentException ex           => (StatusCodes.Status400BadRequest, "Validation error", ex.Message),
            KeyNotFoundException ex        => (StatusCodes.Status404NotFound,   "Resource not found", ex.Message),
            IdempotencyConflictException ex=> (StatusCodes.Status409Conflict,   "Idempotency conflict", ex.Message),
            InvalidOperationException ex   => (StatusCodes.Status409Conflict,   "Operation conflict", ex.Message),
            UnauthorizedAccessException    => (StatusCodes.Status401Unauthorized,"Unauthorized", "Authentication is required."),
            _                              => (StatusCodes.Status500InternalServerError, "Internal server error", "An unexpected error occurred.")
        };

        if (statusCode >= 500)
            logger.LogError(exception, "Unhandled exception on {Method} {Path}", httpContext.Request.Method, httpContext.Request.Path);
        else
            logger.LogWarning("Request failed with {StatusCode}: {Title}", statusCode, title);

        var problem = new ProblemDetails
        {
            Status   = statusCode,
            Title    = title,
            Detail   = detail,
            Instance = httpContext.Request.Path,
            Type     = $"https://httpstatuses.com/{statusCode}",
        };
        problem.Extensions["traceId"] = Activity.Current?.Id ?? httpContext.TraceIdentifier;

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(
            problem,
            options: null,
            contentType: "application/problem+json",
            cancellationToken: cancellationToken);
        return true;
    }
}

/// <summary>DI &amp; pipeline registration helpers for RFC 9457 Problem Details.</summary>
public static class ProblemDetailsExtensions
{
    public static IServiceCollection AddApiProblemDetails(this IServiceCollection services)
    {
        services.AddProblemDetails();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        return services;
    }
}
