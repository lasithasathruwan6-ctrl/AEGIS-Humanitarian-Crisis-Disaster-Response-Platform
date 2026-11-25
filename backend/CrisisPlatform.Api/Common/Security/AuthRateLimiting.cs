using System.Threading.RateLimiting;

namespace CrisisPlatform.Api.Common.Security;

public sealed class AuthRateLimitOptions
{
    public const string SectionName = "RateLimiting:Auth";

    /// <summary>Maximum attempts per client per endpoint within one window.</summary>
    public int PermitLimit { get; set; } = 5;

    public int WindowSeconds { get; set; } = 60;
}

public static class AuthRateLimiting
{
    /// <summary>Paths that are throttled per client address (login and registration are counted separately).</summary>
    public static readonly string[] ProtectedPaths = ["/api/v1/auth/login", "/api/v1/auth/register"];

    public static IServiceCollection AddAuthRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        var options = new AuthRateLimitOptions();
        configuration.GetSection(AuthRateLimitOptions.SectionName).Bind(options);

        if (options.PermitLimit < 1 || options.WindowSeconds < 1)
        {
            throw new InvalidOperationException(
                $"'{AuthRateLimitOptions.SectionName}' PermitLimit and WindowSeconds must be positive.");
        }

        services.AddRateLimiter(limiter =>
        {
            limiter.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            limiter.OnRejected = (context, _) =>
            {
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    context.HttpContext.Response.Headers.RetryAfter =
                        ((int)Math.Ceiling(retryAfter.TotalSeconds)).ToString();
                }

                return ValueTask.CompletedTask;
            };

            limiter.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
            {
                var path = context.Request.Path.Value?.TrimEnd('/').ToLowerInvariant() ?? string.Empty;

                if (!ProtectedPaths.Contains(path) || !HttpMethods.IsPost(context.Request.Method))
                {
                    return RateLimitPartition.GetNoLimiter("unprotected");
                }

                var client = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                return RateLimitPartition.GetFixedWindowLimiter($"{path}|{client}", _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = options.PermitLimit,
                    Window = TimeSpan.FromSeconds(options.WindowSeconds),
                    QueueLimit = 0,
                });
            });
        });

        return services;
    }
}
