using System.Security.Claims;
using CrisisPlatform.Api.Common.Security.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace CrisisPlatform.Tests.Security;

public class PolicyMatrixTests
{
    private static readonly string[] AllRoles = ["Citizen", "Volunteer", "Staff", "Coordinator", "Admin"];

    // Mirrors docs/security/role-endpoint-matrix.md; written out literally so a code change cannot silently change the test.
    private static readonly Dictionary<string, string[]> Allowed = new()
    {
        ["Authenticated"] = ["Citizen", "Volunteer", "Staff", "Coordinator", "Admin"],
        ["Citizen"] = ["Citizen"],
        ["Staff"] = ["Staff", "Coordinator", "Admin"],
        ["Coordinator"] = ["Coordinator", "Admin"],
        ["Admin"] = ["Admin"],
    };

    private static readonly ServiceProvider Services = new ServiceCollection()
        .AddLogging()
        .AddAppAuthorization()
        .BuildServiceProvider();

    public static TheoryData<string, string, bool> Matrix()
    {
        var data = new TheoryData<string, string, bool>();
        foreach (var (policy, roles) in Allowed)
        {
            foreach (var role in AllRoles)
            {
                data.Add(policy, role, roles.Contains(role));
            }
        }

        return data;
    }

    private static ClaimsPrincipal User(string? role) => role is null
        ? new ClaimsPrincipal(new ClaimsIdentity())
        : new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim("sub", "user-1"), new Claim(ClaimTypes.Role, role)], "test"));

    private static async Task<bool> Authorize(ClaimsPrincipal user, string policy)
        => (await Services.GetRequiredService<IAuthorizationService>().AuthorizeAsync(user, policy)).Succeeded;

    [Theory]
    [MemberData(nameof(Matrix))]
    public async Task Role_is_allowed_only_where_the_matrix_says(string policy, string role, bool expected)
    {
        Assert.Equal(expected, await Authorize(User(role), policy));
    }

    [Theory]
    [InlineData("Staff")]
    [InlineData("Coordinator")]
    [InlineData("Admin")]
    public async Task Citizen_is_denied_every_staff_level_policy(string policy)
    {
        Assert.False(await Authorize(User("Citizen"), policy));
    }

    [Theory]
    [InlineData("Authenticated")]
    [InlineData("Citizen")]
    [InlineData("Staff")]
    [InlineData("Coordinator")]
    [InlineData("Admin")]
    public async Task Anonymous_user_is_denied_every_policy(string policy)
    {
        Assert.False(await Authorize(User(null), policy));
    }

    [Fact]
    public async Task User_with_an_unknown_role_is_denied_everything_but_authentication()
    {
        var stranger = User("Hacker");

        Assert.True(await Authorize(stranger, "Authenticated"));
        Assert.False(await Authorize(stranger, "Citizen"));
        Assert.False(await Authorize(stranger, "Staff"));
        Assert.False(await Authorize(stranger, "Admin"));
    }

    [Fact]
    public async Task Endpoints_without_a_policy_require_a_signed_in_user()
    {
        var provider = Services.GetRequiredService<IAuthorizationPolicyProvider>();
        var fallback = await provider.GetFallbackPolicyAsync();
        var auth = Services.GetRequiredService<IAuthorizationService>();

        Assert.NotNull(fallback);
        Assert.False((await auth.AuthorizeAsync(User(null), null, fallback)).Succeeded);
        Assert.True((await auth.AuthorizeAsync(User("Citizen"), null, fallback)).Succeeded);
    }
}
