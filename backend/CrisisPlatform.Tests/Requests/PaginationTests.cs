using System.Net.Http.Json;
using CrisisPlatform.Api.Common.Pagination;
using CrisisPlatform.Api.Modules.Requests;
using CrisisPlatform.Tests.Security;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace CrisisPlatform.Tests.Requests;

/// <summary>Unit and HTTP tests for pagination on list endpoints.</summary>
public sealed class PaginationTests
{
    [Fact]
    public void PaginationQuery_ClampsOutOfRangeValues()
    {
        var low  = new PaginationQuery(page: 0, pageSize: -5);
        var high = new PaginationQuery(page: 3, pageSize: 500);

        Assert.Equal(1, low.Page);
        Assert.Equal(PaginationQuery.DefaultPageSize, low.PageSize);
        Assert.Equal(PaginationQuery.MaxPageSize, high.PageSize);
    }

    [Fact]
    public void ToPagedResult_ComputesMetadataAndSlicesCorrectly()
    {
        var items = Enumerable.Range(1, 25).ToList();
        var page2 = items.ToPagedResult(new PaginationQuery(page: 2, pageSize: 10));

        Assert.Equal(10, page2.Items.Count);
        Assert.Equal(11, page2.Items[0]);
        Assert.Equal(20, page2.Items[^1]);
        Assert.Equal(25, page2.TotalCount);
        Assert.Equal(3,  page2.TotalPages);
        Assert.True(page2.HasPrevious);
        Assert.True(page2.HasNext);
    }

    [Fact]
    public async Task GetRequests_ReturnsPagedResult_ViaHttp()
    {
        var service = new RequestService(new ManualTimeProvider(new DateTimeOffset(2026, 6, 1, 10, 0, 0, TimeSpan.Zero)));
        for (int i = 1; i <= 5; i++)
        {
            service.Create(
                new CreateRequestDto($"hash-000000000000000{i}", "Colombo", "Water", 3, $"Need water batch #{i}", "0771234567"),
                $"key-{i}");
        }

        using var client = await TestHostFactory.CreateClientAsync(
            builder =>
            {
                builder.Services.AddControllers()
                    .AddApplicationPart(typeof(RequestsController).Assembly);
                builder.Services.AddSingleton(service);
            },
            app =>
            {
                app.UseRouting();
                app.MapControllers();
            });

        var page1 = await client.GetFromJsonAsync<PagedResult<RequestDto>>("/api/v1/requests?page=1&pageSize=2");
        var page3 = await client.GetFromJsonAsync<PagedResult<RequestDto>>("/api/v1/requests?page=3&pageSize=2");

        Assert.NotNull(page1);
        Assert.Equal(2, page1.Items.Count);
        Assert.Equal(5, page1.TotalCount);
        Assert.Equal(3, page1.TotalPages);
        Assert.True(page1.HasNext);
        Assert.False(page1.HasPrevious);

        Assert.NotNull(page3);
        Assert.Single(page3.Items);
        Assert.False(page3.HasNext);
        Assert.True(page3.HasPrevious);
    }
}
