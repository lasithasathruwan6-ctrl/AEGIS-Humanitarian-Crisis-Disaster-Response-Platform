using CrisisPlatform.Api.Common.Pagination;
using CrisisPlatform.Api.Modules.Requests;

namespace CrisisPlatform.Tests.Database;

public sealed class RequestQueueProjectionTests
{
    private sealed class IncrementingClock(DateTimeOffset start) : TimeProvider
    {
        private DateTimeOffset _now = start;

        public override DateTimeOffset GetUtcNow()
        {
            var current = _now;
            _now = _now.AddMinutes(1);
            return current;
        }
    }

    [Fact]
    public void ListQueue_filters_by_status_and_district_and_orders_by_urgency_then_created_at()
    {
        var clock = new IncrementingClock(new DateTimeOffset(2026, 6, 1, 8, 0, 0, TimeSpan.Zero));
        var service = new RequestService(clock);

        var lowColombo = service.Create(
            new CreateRequestDto("hash-1", "Colombo", "Food", 2, "Need rations", "+94770000001"),
            "key-1");
        var highKandy = service.Create(
            new CreateRequestDto("hash-2", "Kandy", "Medical", 5, "Insulin needed", "+94770000002"),
            "key-2");
        var highColomboFirst = service.Create(
            new CreateRequestDto("hash-3", "Colombo", "Water", 5, "Clean water", "+94770000003"),
            "key-3");
        var highColomboSecond = service.Create(
            new CreateRequestDto("hash-4", "Colombo", "Shelter", 5, "Roof damaged", "+94770000004"),
            "key-4");

        var page = service.ListQueue(new PaginationQuery(page: 1, pageSize: 2), status: "Pending", district: "Colombo");

        Assert.Equal(3, page.TotalCount);
        Assert.Equal(2, page.Items.Count);
        Assert.Equal(highColomboFirst.Id, page.Items[0].Id);
        Assert.Equal(highColomboSecond.Id, page.Items[1].Id);
        Assert.DoesNotContain(page.Items, x => x.Id == highKandy.Id || x.Id == lowColombo.Id);
    }
}
