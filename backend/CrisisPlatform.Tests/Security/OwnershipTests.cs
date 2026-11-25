using System.Security.Claims;
using CrisisPlatform.Api.Common.Security.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace CrisisPlatform.Tests.Security;

public class OwnershipTests
{
    private sealed record OwnedRequest(string OwnerId, string Title, string? AssignedUserId = null) : IOwnedResource;

    private sealed record OwnedConversation(string OwnerId, string? AssignedUserId = null) : IOwnedResource;

    private static readonly ServiceProvider Services = new ServiceCollection()
        .AddLogging()
        .AddAppAuthorization()
        .AddOwnershipAuthorization()
        .BuildServiceProvider();

    private static ClaimsPrincipal User(string role, string? id = "citizen-a") => new(new ClaimsIdentity(
        id is null
            ? [new Claim(ClaimTypes.Role, role)]
            : [new Claim("sub", id), new Claim(ClaimTypes.Role, role)],
        "test"));

    private static async Task<bool> CanRead(ClaimsPrincipal user, IOwnedResource resource)
        => (await Services.GetRequiredService<IAuthorizationService>()
            .AuthorizeAsync(user, resource, OwnershipExtensions.PolicyName)).Succeeded;

    [Fact]
    public async Task Citizen_can_read_their_own_request_and_conversation()
    {
        Assert.True(await CanRead(User("Citizen"), new OwnedRequest("citizen-a", "Need water")));
        Assert.True(await CanRead(User("Citizen"), new OwnedConversation("citizen-a")));
    }

    [Fact]
    public async Task Citizen_cannot_read_another_citizens_request_or_conversation()
    {
        Assert.False(await CanRead(User("Citizen"), new OwnedRequest("citizen-b", "Need food")));
        Assert.False(await CanRead(User("Citizen"), new OwnedConversation("citizen-b")));
    }

    [Theory]
    [InlineData("Staff")]
    [InlineData("Coordinator")]
    [InlineData("Admin")]
    public async Task Staff_roles_can_read_any_request(string role)
    {
        Assert.True(await CanRead(User(role, "staff-1"), new OwnedRequest("citizen-b", "Need food")));
    }

    [Fact]
    public async Task Volunteer_can_read_only_records_assigned_to_them()
    {
        var volunteer = User("Volunteer", "vol-1");

        Assert.True(await CanRead(volunteer, new OwnedRequest("citizen-b", "Need food", "vol-1")));
        Assert.False(await CanRead(volunteer, new OwnedRequest("citizen-b", "Need food")));
        Assert.False(await CanRead(volunteer, new OwnedRequest("citizen-b", "Need food", "vol-2")));
    }

    [Fact]
    public async Task User_without_a_subject_claim_is_denied()
    {
        Assert.False(await CanRead(User("Citizen", id: null), new OwnedRequest("citizen-a", "Need water")));
    }

    [Fact]
    public async Task Anonymous_user_is_denied()
    {
        var anonymous = new ClaimsPrincipal(new ClaimsIdentity());

        Assert.False(await CanRead(anonymous, new OwnedRequest("citizen-a", "Need water")));
    }

    [Fact]
    public async Task Citizen_owning_a_record_only_by_assignment_field_is_not_enough()
    {
        // AssignedUserId is for volunteers; a citizen cannot gain access by being named there.
        Assert.False(await CanRead(User("Citizen"), new OwnedRequest("citizen-b", "Need food", "citizen-a")));
    }

    [Fact]
    public void Citizen_query_returns_only_their_own_requests()
    {
        var all = new[]
        {
            new OwnedRequest("citizen-a", "A1"),
            new OwnedRequest("citizen-b", "B1"),
            new OwnedRequest("citizen-a", "A2"),
        }.AsQueryable();

        var visible = OwnerScope.For(all, User("Citizen")).Select(r => r.Title).ToList();

        Assert.Equal(["A1", "A2"], visible);
    }

    [Fact]
    public void Citizen_query_returns_only_their_own_conversations()
    {
        var all = new[] { new OwnedConversation("citizen-a"), new OwnedConversation("citizen-b") }.AsQueryable();

        var visible = OwnerScope.For(all, User("Citizen")).ToList();

        Assert.Equal("citizen-a", Assert.Single(visible).OwnerId);
    }

    [Fact]
    public void Staff_query_returns_everything_and_anonymous_query_returns_nothing()
    {
        var all = new[] { new OwnedRequest("citizen-a", "A1"), new OwnedRequest("citizen-b", "B1") }.AsQueryable();

        Assert.Equal(2, OwnerScope.For(all, User("Staff", "staff-1")).Count());
        Assert.Empty(OwnerScope.For(all, new ClaimsPrincipal(new ClaimsIdentity())));
        Assert.Empty(OwnerScope.For(all, User("Citizen", id: null)));
    }

    [Fact]
    public void Volunteer_query_returns_only_assigned_records()
    {
        var all = new[]
        {
            new OwnedRequest("citizen-a", "A1", "vol-1"),
            new OwnedRequest("citizen-b", "B1", "vol-2"),
            new OwnedRequest("citizen-c", "C1"),
        }.AsQueryable();

        var visible = OwnerScope.For(all, User("Volunteer", "vol-1")).Select(r => r.Title).ToList();

        Assert.Equal(["A1"], visible);
    }
}
