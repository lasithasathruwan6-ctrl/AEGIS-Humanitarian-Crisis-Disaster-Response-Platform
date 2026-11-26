using System.Net;
using System.Net.Http.Json;
using CrisisPlatform.Api.Common.Errors;
using CrisisPlatform.Api.Modules.Requests;
using CrisisPlatform.Tests.Security;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace CrisisPlatform.Tests.Requests;

/// <summary>Tests for <c>Idempotency-Key</c> handling on aid-request creation.</summary>
public sealed class IdempotencyKeyTests
{
    private static CreateRequestDto SampleDto(string need = "Water") => new(
        NicHash:      "hash-1234567890abcdef",
        District:     "Colombo",
        NeedType:     need,
        Urgency:      4,
        Description:  "Urgent clean drinking water needed.",
        ContactPhone: "0771234567");

    private static Task<HttpClient> CreateClientAsync() =>
        TestHostFactory.CreateClientAsync(
            builder =>
            {
                builder.Services.AddControllers()
                    .AddApplicationPart(typeof(RequestsController).Assembly);
                builder.Services.AddApiProblemDetails();
                builder.Services.AddSingleton<TimeProvider>(
                    new ManualTimeProvider(new DateTimeOffset(2026, 6, 1, 10, 0, 0, TimeSpan.Zero)));
                builder.Services.AddSingleton<RequestService>();
            },
            app =>
            {
                app.UseExceptionHandler();
                app.UseRouting();
                app.MapControllers();
            });

    [Fact]
    public async Task PostRequest_Returns400_WhenIdempotencyKeyMissing()
    {
        using var client = await CreateClientAsync();
        var response = await client.PostAsJsonAsync("/api/v1/requests", SampleDto());

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostRequest_ReturnsSameRequestId_WhenIdempotencyKeyReplayedWithSamePayload()
    {
        using var client = await CreateClientAsync();

        using var req1 = new HttpRequestMessage(HttpMethod.Post, "/api/v1/requests")
        {
            Content = JsonContent.Create(SampleDto()),
        };
        req1.Headers.Add("Idempotency-Key", "idem-key-001");
        var res1 = await client.SendAsync(req1);

        using var req2 = new HttpRequestMessage(HttpMethod.Post, "/api/v1/requests")
        {
            Content = JsonContent.Create(SampleDto()),
        };
        req2.Headers.Add("Idempotency-Key", "idem-key-001");
        var res2 = await client.SendAsync(req2);

        Assert.Equal(HttpStatusCode.Created, res1.StatusCode);
        Assert.Equal(HttpStatusCode.Created, res2.StatusCode);

        var dto1 = await res1.Content.ReadFromJsonAsync<RequestDto>();
        var dto2 = await res2.Content.ReadFromJsonAsync<RequestDto>();

        Assert.NotNull(dto1);
        Assert.NotNull(dto2);
        Assert.Equal(dto1.Id, dto2.Id);
    }

    [Fact]
    public async Task PostRequest_Returns409Conflict_WhenIdempotencyKeyReusedWithDifferentPayload()
    {
        using var client = await CreateClientAsync();

        using var req1 = new HttpRequestMessage(HttpMethod.Post, "/api/v1/requests")
        {
            Content = JsonContent.Create(SampleDto("Water")),
        };
        req1.Headers.Add("Idempotency-Key", "idem-key-002");
        var res1 = await client.SendAsync(req1);
        Assert.Equal(HttpStatusCode.Created, res1.StatusCode);

        using var req2 = new HttpRequestMessage(HttpMethod.Post, "/api/v1/requests")
        {
            Content = JsonContent.Create(SampleDto("Medical")),
        };
        req2.Headers.Add("Idempotency-Key", "idem-key-002");
        var res2 = await client.SendAsync(req2);

        Assert.Equal(HttpStatusCode.Conflict, res2.StatusCode);
    }

    [Fact]
    public async Task PostRequest_CreatesDistinctRequests_ForDifferentIdempotencyKeys()
    {
        using var client = await CreateClientAsync();

        using var req1 = new HttpRequestMessage(HttpMethod.Post, "/api/v1/requests")
        {
            Content = JsonContent.Create(SampleDto()),
        };
        req1.Headers.Add("Idempotency-Key", "idem-key-A");
        var res1 = await client.SendAsync(req1);

        using var req2 = new HttpRequestMessage(HttpMethod.Post, "/api/v1/requests")
        {
            Content = JsonContent.Create(SampleDto()),
        };
        req2.Headers.Add("Idempotency-Key", "idem-key-B");
        var res2 = await client.SendAsync(req2);

        var dto1 = await res1.Content.ReadFromJsonAsync<RequestDto>();
        var dto2 = await res2.Content.ReadFromJsonAsync<RequestDto>();

        Assert.NotEqual(dto1!.Id, dto2!.Id);
    }
}
