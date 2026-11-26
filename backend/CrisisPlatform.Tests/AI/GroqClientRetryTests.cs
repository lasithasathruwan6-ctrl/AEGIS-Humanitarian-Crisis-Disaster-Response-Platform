using System.Net;
using System.Text;
using CrisisPlatform.Api.Modules.AI;

namespace CrisisPlatform.Tests.AI;

public class GroqClientRetryTests
{
    private sealed class MockHandler(Func<int, HttpResponseMessage> responder) : HttpMessageHandler
    {
        public int CallCount { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            CallCount++;
            return Task.FromResult(responder(CallCount));
        }
    }

    [Fact]
    public async Task AskAssistant_WhenGroqReturns429Then200_ShouldRetryAndSucceed()
    {
        var handler = new MockHandler(attempt => attempt == 1
            ? new HttpResponseMessage(HttpStatusCode.TooManyRequests)
            : new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"choices\":[{\"message\":{\"content\":\"Move to higher ground.\"}}]}", Encoding.UTF8, "application/json")
            });

        var client = new GroqAssistantClient(new HttpClient(handler), apiKey: "test-key");
        var res = await client.AskAssistantAsync("Flood water is rising near our house.");

        Assert.Equal(2, handler.CallCount);
        Assert.False(res.UsedFallback);
        Assert.Contains("Move to higher ground.", res.Reply);
    }

    [Fact]
    public async Task AskAssistant_WhenGroqTimesOut_ShouldFallbackSafely()
    {
        var handler = new MockHandler(_ => throw new TaskCanceledException("Simulated HTTP timeout"));
        var client = new GroqAssistantClient(new HttpClient(handler), apiKey: "test-key");

        var res = await client.AskAssistantAsync("Need flood safety steps.");

        Assert.True(res.UsedFallback);
        Assert.True(res.IsValid);
        Assert.Contains("DMC", res.Reply);
    }
}
