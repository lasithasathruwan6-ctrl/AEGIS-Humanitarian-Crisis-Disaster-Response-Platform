namespace CrisisPlatform.Api.Common.Security;

/// <summary>Adds defensive HTTP response headers to every response.</summary>
public sealed class SecurityHeadersMiddleware(RequestDelegate next)
{
    private static readonly (string Name, string Value)[] Headers =
    [
        ("X-Content-Type-Options", "nosniff"),
        ("X-Frame-Options", "DENY"),
        ("Referrer-Policy", "no-referrer"),
        ("Content-Security-Policy", "default-src 'none'; frame-ancestors 'none'"),
        ("Permissions-Policy", "camera=(), microphone=(), geolocation=()"),
        ("Cross-Origin-Resource-Policy", "same-site"),
    ];

    public Task InvokeAsync(HttpContext context)
    {
        context.Response.OnStarting(() =>
        {
            foreach (var (name, value) in Headers)
            {
                context.Response.Headers[name] = value;
            }

            return Task.CompletedTask;
        });

        return next(context);
    }
}

public static class SecurityHeadersExtensions
{
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
        => app.UseMiddleware<SecurityHeadersMiddleware>();
}
