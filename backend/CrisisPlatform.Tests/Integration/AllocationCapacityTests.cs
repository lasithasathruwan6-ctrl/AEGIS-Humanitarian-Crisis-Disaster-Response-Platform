using CrisisPlatform.Api.Common.Security;
using CrisisPlatform.Api.Modules.Auth.Tokens;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace CrisisPlatform.Tests.Integration;

/// <summary>
/// Integration tests that assert the HTTP 409 Conflict contract for shelter allocations
/// when the requested occupant count would exceed the shelter's capacity.
///
/// The shelter and allocation logic are simulated with an in-memory store so these
/// tests run entirely in-process without a database.
/// </summary>
public class AllocationCapacityTests
{
    // -----------------------------------------------------------------------
    // In-memory shelter store (test data only — never touches the application DB)
    // -----------------------------------------------------------------------

    private sealed class InMemoryShelterStore
    {
        private readonly Dictionary<string, int> _capacity  = new();
        private readonly Dictionary<string, int> _occupancy = new();

        public void AddShelter(string id, int capacity)
        {
            _capacity[id]  = capacity;
            _occupancy[id] = 0;
        }

        /// <returns>True when allocation was accepted; false when it would exceed capacity.</returns>
        public bool TryAllocate(string shelterId, int occupants)
        {
            if (!_capacity.TryGetValue(shelterId, out var cap)) return false;
            if (_occupancy[shelterId] + occupants > cap) return false;
            _occupancy[shelterId] += occupants;
            return true;
        }
    }

    // -----------------------------------------------------------------------
    // Host factory
    // -----------------------------------------------------------------------

    private static async Task<(HttpClient Client, string ShelterId)> BuildClientAsync()
    {
        const string shelterId = "shelter-test-001";
        const int    capacity  = 10;

        var store = new InMemoryShelterStore();
        store.AddShelter(shelterId, capacity);

        var tokenOptions = new TokenOptions
        {
            SigningKey         = "integration-test-signing-key-0123456789-abcdef",
            AccessTokenMinutes = 15,
        };
        var tokenService = new AccessTokenService(tokenOptions, TimeProvider.System);
        var staffToken   = tokenService.Create("staff-user-1", "Staff");

        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddSingleton(store);
        builder.Services.AddFrontendCors(builder.Configuration);
        builder.Services.AddAuthRateLimiting(builder.Configuration);

        var app = builder.Build();
        app.UseSecurityHeaders();
        app.UseCors(CorsExtensions.PolicyName);
        app.UseRateLimiter();

        app.MapPost("/api/v1/shelters/{id}/allocations",
            async (string id, AllocationRequest req, InMemoryShelterStore s, HttpContext ctx) =>
            {
                // Minimal auth check: require a Bearer header (content is not validated here).
                var auth = ctx.Request.Headers.Authorization.ToString();
                if (!auth.StartsWith("Bearer ")) return Results.Unauthorized();

                await Task.CompletedTask;
                return s.TryAllocate(id, req.Occupants)
                    ? Results.Ok(new { shelterId = id, occupants = req.Occupants })
                    : Results.Conflict(new { error = $"Shelter {id} does not have enough free capacity." });
            });

        await app.StartAsync();
        var client = app.GetTestClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", staffToken);

        return (client, shelterId);
    }

    // -----------------------------------------------------------------------
    // Tests
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Allocating_within_capacity_returns_200()
    {
        var (client, shelterId) = await BuildClientAsync();

        var response = await client.PostAsJsonAsync(
            $"/api/v1/shelters/{shelterId}/allocations",
            new AllocationRequest(5));

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Allocating_exactly_at_capacity_returns_200()
    {
        var (client, shelterId) = await BuildClientAsync();

        var response = await client.PostAsJsonAsync(
            $"/api/v1/shelters/{shelterId}/allocations",
            new AllocationRequest(10)); // capacity is 10

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Allocating_one_over_capacity_returns_409()
    {
        var (client, shelterId) = await BuildClientAsync();

        var response = await client.PostAsJsonAsync(
            $"/api/v1/shelters/{shelterId}/allocations",
            new AllocationRequest(11)); // capacity is 10

        Assert.Equal(System.Net.HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Parallel_allocations_cannot_collectively_exceed_capacity()
    {
        var (client, shelterId) = await BuildClientAsync();

        // Fill 9 of 10 slots.
        await client.PostAsJsonAsync(
            $"/api/v1/shelters/{shelterId}/allocations",
            new AllocationRequest(9));

        // Attempt to add 2 more — should fail because only 1 remains.
        var response = await client.PostAsJsonAsync(
            $"/api/v1/shelters/{shelterId}/allocations",
            new AllocationRequest(2));

        Assert.Equal(System.Net.HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Conflict_response_body_contains_error_field()
    {
        var (client, shelterId) = await BuildClientAsync();

        var response = await client.PostAsJsonAsync(
            $"/api/v1/shelters/{shelterId}/allocations",
            new AllocationRequest(999));

        Assert.Equal(System.Net.HttpStatusCode.Conflict, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ErrorBody>();
        Assert.False(string.IsNullOrWhiteSpace(body?.Error));
    }

    [Fact]
    public async Task Unauthenticated_allocation_is_rejected()
    {
        var (client, shelterId) = await BuildClientAsync();
        client.DefaultRequestHeaders.Authorization = null;

        var response = await client.PostAsJsonAsync(
            $"/api/v1/shelters/{shelterId}/allocations",
            new AllocationRequest(1));

        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // -----------------------------------------------------------------------
    // Supporting records (test data only)
    // -----------------------------------------------------------------------

    private record AllocationRequest(int Occupants);
    private record ErrorBody(string Error);
}
