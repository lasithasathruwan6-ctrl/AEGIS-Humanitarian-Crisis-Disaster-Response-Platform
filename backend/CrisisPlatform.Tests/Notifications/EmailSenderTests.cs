using System.Net;
using CrisisPlatform.Api.Modules.Notifications;
using Microsoft.Extensions.Logging.Abstractions;

namespace CrisisPlatform.Tests.Notifications;

/// <summary>Unit tests for <see cref="HttpEmailSender"/> abstraction and failure handling.</summary>
public sealed class EmailSenderTests
{
    private sealed class StubHandler(Func<HttpRequestMessage, HttpResponseMessage> fn) : HttpMessageHandler
    {
        public HttpRequestMessage? LastRequest { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            return Task.FromResult(fn(request));
        }
    }

    [Fact]
    public async Task SendAsync_ReturnsSuccessAndMessageId_On202Accepted()
    {
        var stub = new StubHandler(_ =>
        {
            var msg = new HttpResponseMessage(HttpStatusCode.Accepted);
            msg.Headers.Add("X-Message-Id", "msg-999");
            return msg;
        });

        using var http = new HttpClient(stub) { BaseAddress = new Uri("https://mail.example.test/") };
        IEmailSender sut = new HttpEmailSender(http, NullLogger<HttpEmailSender>.Instance);

        var result = await sut.SendAsync(new EmailPayload("citizen@example.lk", "Alert", "Body", "idem-1"));

        Assert.True(result.Succeeded);
        Assert.Equal("msg-999", result.ProviderMessageId);
        Assert.Null(result.Error);
        Assert.True(stub.LastRequest!.Headers.Contains("Idempotency-Key"));
    }

    [Fact]
    public async Task SendAsync_ReturnsFailureResult_OnProvider500()
    {
        var stub = new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError));
        using var http = new HttpClient(stub) { BaseAddress = new Uri("https://mail.example.test/") };
        IEmailSender sut = new HttpEmailSender(http, NullLogger<HttpEmailSender>.Instance);

        var result = await sut.SendAsync(new EmailPayload("citizen@example.lk", "Alert", "Body"));

        Assert.False(result.Succeeded);
        Assert.Contains("500", result.Error);
    }

    [Fact]
    public async Task SendAsync_ReturnsFailureResult_OnTransportException()
    {
        var stub = new StubHandler(_ => throw new HttpRequestException("SMTP relay unreachable"));
        using var http = new HttpClient(stub) { BaseAddress = new Uri("https://mail.example.test/") };
        IEmailSender sut = new HttpEmailSender(http, NullLogger<HttpEmailSender>.Instance);

        var result = await sut.SendAsync(new EmailPayload("citizen@example.lk", "Alert", "Body"));

        Assert.False(result.Succeeded);
        Assert.Contains("SMTP relay unreachable", result.Error);
    }
}
