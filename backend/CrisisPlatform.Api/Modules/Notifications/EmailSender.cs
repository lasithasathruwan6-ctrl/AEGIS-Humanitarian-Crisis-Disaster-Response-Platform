using System.Net;
using System.Net.Http.Json;

namespace CrisisPlatform.Api.Modules.Notifications;

public sealed record EmailPayload(string To, string Subject, string Body, string? IdempotencyKey = null);

public sealed record EmailSendResult(bool Succeeded, string? ProviderMessageId, string? Error);

/// <summary>Abstraction over outbound email delivery providers.</summary>
public interface IEmailSender
{
    Task<EmailSendResult> SendAsync(EmailPayload payload, CancellationToken cancellationToken = default);
}

/// <summary>HTTP-based email sender with structured failure handling (never throws on 4xx/5xx/network errors).</summary>
public sealed class HttpEmailSender(HttpClient httpClient, ILogger<HttpEmailSender> logger) : IEmailSender
{
    public async Task<EmailSendResult> SendAsync(EmailPayload payload, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(payload.To);
        ArgumentException.ThrowIfNullOrWhiteSpace(payload.Subject);

        try
        {
            using var req = new HttpRequestMessage(HttpMethod.Post, "send")
            {
                Content = JsonContent.Create(payload),
            };
            if (!string.IsNullOrWhiteSpace(payload.IdempotencyKey))
                req.Headers.Add("Idempotency-Key", payload.IdempotencyKey);

            using var res = await httpClient.SendAsync(req, cancellationToken);
            if (res.IsSuccessStatusCode)
            {
                var messageId = res.Headers.TryGetValues("X-Message-Id", out var vals)
                    ? vals.FirstOrDefault()
                    : Guid.NewGuid().ToString("N");
                return new EmailSendResult(true, messageId, null);
            }

            logger.LogWarning("Email delivery failed with HTTP {StatusCode} for recipient domain.", (int)res.StatusCode);
            return new EmailSendResult(false, null, $"Provider returned HTTP {(int)res.StatusCode}");
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            logger.LogWarning(ex, "Email delivery failed due to transport error.");
            return new EmailSendResult(false, null, ex.Message);
        }
    }
}
