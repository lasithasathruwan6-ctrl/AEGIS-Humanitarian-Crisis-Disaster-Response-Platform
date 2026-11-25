namespace CrisisPlatform.Api.Common.Security;

public static class CorsExtensions
{
    public const string PolicyName = "Frontend";
    public const string AllowedOriginKey = "Cors:AllowedOrigin";

    /// <summary>
    /// Registers a CORS policy that allows exactly one origin, taken from configuration.
    /// Startup fails if the origin is missing, a wildcard or not an absolute http(s) origin.
    /// </summary>
    public static IServiceCollection AddFrontendCors(this IServiceCollection services, IConfiguration configuration)
    {
        var origin = configuration[AllowedOriginKey]?.Trim().TrimEnd('/');

        if (string.IsNullOrEmpty(origin)
            || origin.Contains('*')
            || !Uri.TryCreate(origin, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
            || uri.PathAndQuery != "/")
        {
            throw new InvalidOperationException(
                $"'{AllowedOriginKey}' must be a single absolute http(s) origin such as https://app.example.org; wildcards are not allowed.");
        }

        services.AddCors(options => options.AddPolicy(PolicyName, policy => policy
            .WithOrigins(origin)
            .WithMethods("GET", "POST", "PUT", "PATCH", "DELETE")
            .WithHeaders("Authorization", "Content-Type")));

        return services;
    }
}
