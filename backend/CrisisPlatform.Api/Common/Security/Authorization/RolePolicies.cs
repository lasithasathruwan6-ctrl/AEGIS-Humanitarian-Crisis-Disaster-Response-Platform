using Microsoft.AspNetCore.Authorization;

namespace CrisisPlatform.Api.Common.Security.Authorization;

public static class Roles
{
    public const string Citizen = "Citizen";
    public const string Volunteer = "Volunteer";
    public const string Staff = "Staff";
    public const string Coordinator = "Coordinator";
    public const string Admin = "Admin";

    /// <summary>Roles that work on behalf of the organisation and may see all records.</summary>
    public static readonly string[] StaffRoles = [Staff, Coordinator, Admin];
}

public static class Policies
{
    public const string Authenticated = "Authenticated";
    public const string Citizen = "Citizen";
    public const string Staff = "Staff";
    public const string Coordinator = "Coordinator";
    public const string Admin = "Admin";
}

public static class AuthorizationExtensions
{
    /// <summary>
    /// Registers the role policies from docs/security/role-endpoint-matrix.md.
    /// Endpoints without an explicit policy fall back to "authenticated user required" (deny by default).
    /// </summary>
    public static IServiceCollection AddAppAuthorization(this IServiceCollection services)
    {
        services.AddAuthorizationBuilder()
            .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build())
            .AddPolicy(Policies.Authenticated, p => p.RequireAuthenticatedUser())
            .AddPolicy(Policies.Citizen, p => p.RequireAuthenticatedUser().RequireRole(Roles.Citizen))
            .AddPolicy(Policies.Staff, p => p.RequireAuthenticatedUser().RequireRole(Roles.StaffRoles))
            .AddPolicy(Policies.Coordinator, p => p.RequireAuthenticatedUser().RequireRole(Roles.Coordinator, Roles.Admin))
            .AddPolicy(Policies.Admin, p => p.RequireAuthenticatedUser().RequireRole(Roles.Admin));

        return services;
    }
}
