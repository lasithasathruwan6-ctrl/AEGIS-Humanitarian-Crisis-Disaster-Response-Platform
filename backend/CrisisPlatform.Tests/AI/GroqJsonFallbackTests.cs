using System.Net;
using System.Text;
using CrisisPlatform.Api.Modules.AI;

namespace CrisisPlatform.Tests.AI;

public class GroqJsonFallbackTests
{
    private sealed class StaticResponseHandler(string rawContent) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var payload = $"{{\"choices\":[{{\"message\":{{\"content\":{System.Text.Json.JsonSerializer.Serialize(rawContent)}}}}}]}}";
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(payload, Encoding.UTF8, "application/json")
            });
        }
    }

    [Theory]
    [InlineData("not valid json at all")]
    [InlineData("{\"wrongField\": 123}")]
    public async Task TriageWithFallback_WhenModelReturnsInvalidJson_ShouldFallBackToRuleEngine(string invalidModelOutput)
    {
        var client = new GroqAssistantClient(new HttpClient(new StaticResponseHandler(invalidModelOutput)), apiKey: "test-key");
        var input = new TriageInput("Medical", "Family trapped by flood, elderly patient needs insulin.", 5, 4, "elderly");

        var (result, usedFallback) = await client.TriageWithFallbackAsync(input);

        Assert.True(usedFallback);
        Assert.Contains(result.Priority, new[] { "High", "Critical" });
        Assert.True(result.UrgencyScore >= 60);
    }
}
