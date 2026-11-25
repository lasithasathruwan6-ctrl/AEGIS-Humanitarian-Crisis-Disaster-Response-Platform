using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace CrisisPlatform.Api.Common.Security.Authorization;

/// <summary>A record that belongs to one user, such as an aid request or a conversation.</summary>
public interface IOwnedResource
{
    string OwnerId { get; }

    /// <summary>A volunteer assigned to the record, who may also read it.</summary>
    string? AssignedUserId => null;
}

public sealed class OwnerOrStaffRequirement : IAuthorizationRequirement;

public sealed class OwnerOrStaffHandler : AuthorizationHandler<OwnerOrStaffRequirement, IOwnedResource>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, OwnerOrStaffRequirement requirement, IOwnedResource resource)
    {
        if (OwnerScope.Allows(context.User, resource))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

public static class OwnerScope
{
    public static string? GetUserId(ClaimsPrincipal user)
        => user.FindFirst("sub")?.Value ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    public static bool IsStaff(ClaimsPrincipal user) => Roles.StaffRoles.Any(user.IsInRole);

    public static bool Allows(ClaimsPrincipal user, IOwnedResource resource)
    {
        if (user.Identity?.IsAuthenticated != true)
        {
            return false;
        }

        if (IsStaff(user))
        {
            return true;
        }

        var userId = GetUserId(user);
        if (string.IsNullOrEmpty(userId))
        {
            return false;
        }

        if (user.IsInRole(Roles.Citizen))
        {
            return resource.OwnerId == userId;
        }

        return user.IsInRole(Roles.Volunteer) && resource.AssignedUserId == userId;
    }

    /// <summary>
    /// Restricts a query to what the user may read, so ownership is enforced where the data is fetched
    /// and not only after the record has been loaded.
    /// </summary>
    public static IQueryable<T> For<T>(IQueryable<T> query, ClaimsPrincipal user) where T : IOwnedResource
    {
        if (user.Identity?.IsAuthenticated != true)
        {
            return query.Where(_ => false);
        }

        if (IsStaff(user))
        {
            return query;
        }

        var userId = GetUserId(user);
        if (string.IsNullOrEmpty(userId))
        {
            return query.Where(_ => false);
        }

        if (user.IsInRole(Roles.Citizen))
        {
            return query.Where(r => r.OwnerId == userId);
        }

        return user.IsInRole(Roles.Volunteer)
            ? query.Where(r => r.AssignedUserId == userId)
            : query.Where(_ => false);
    }
}

public static class OwnershipExtensions
{
    public const string PolicyName = "OwnerOrStaff";

    /// <summary>Adds the "OwnerOrStaff" policy; authorise with the loaded record as the resource.</summary>
    public static IServiceCollection AddOwnershipAuthorization(this IServiceCollection services)
    {
        services.AddSingleton<IAuthorizationHandler, OwnerOrStaffHandler>();
        services.Configure<AuthorizationOptions>(options => options.AddPolicy(
            PolicyName, policy => policy.RequireAuthenticatedUser().AddRequirements(new OwnerOrStaffRequirement())));

        return services;
    }
}
