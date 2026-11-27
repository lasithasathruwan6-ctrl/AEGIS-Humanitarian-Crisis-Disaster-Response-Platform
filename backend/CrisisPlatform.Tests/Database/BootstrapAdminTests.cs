using CrisisPlatform.Api.Common.Security;
using CrisisPlatform.Api.Common.Security.Authorization;

namespace CrisisPlatform.Tests.Database;

public sealed class BootstrapAdminTests
{
    private sealed record BootstrapUser(Guid Id, string Email, string Role, DateTimeOffset? DeletedAt);

    private static (bool Created, string Reason) TryBootstrapAdmin(
        ICollection<BootstrapUser> users,
        string email,
        string password)
    {
        if (users.Any(u => u.DeletedAt is null && string.Equals(u.Role, Roles.Admin, StringComparison.Ordinal)))
        {
            return (false, "An active Admin already exists.");
        }

        var passwordErrors = PasswordPolicy.Validate(password, email);
        if (passwordErrors.Count > 0)
        {
            return (false, string.Join(" ", passwordErrors));
        }

        users.Add(new BootstrapUser(Guid.NewGuid(), email, Roles.Admin, null));
        return (true, "Bootstrapped.");
    }

    [Fact]
    public void Bootstrap_creates_admin_when_none_exists_and_password_is_valid()
    {
        var users = new List<BootstrapUser>();

        var result = TryBootstrapAdmin(users, "admin@example.org", "V3ry!Str0ng#Crisis99");

        Assert.True(result.Created);
        Assert.Single(users);
        Assert.Equal(Roles.Admin, users[0].Role);
    }

    [Fact]
    public void Bootstrap_is_idempotent_and_rejects_second_admin_when_active_admin_exists()
    {
        var users = new List<BootstrapUser>
        {
            new(Guid.NewGuid(), "existing-admin@example.org", Roles.Admin, null)
        };

        var result = TryBootstrapAdmin(users, "second-admin@example.org", "V3ry!Str0ng#Crisis99");

        Assert.False(result.Created);
        Assert.Equal("An active Admin already exists.", result.Reason);
        Assert.Single(users);
    }

    [Fact]
    public void Bootstrap_rejects_weak_password_before_inserting()
    {
        var users = new List<BootstrapUser>();

        var result = TryBootstrapAdmin(users, "admin@example.org", "short");

        Assert.False(result.Created);
        Assert.Empty(users);
    }
}
