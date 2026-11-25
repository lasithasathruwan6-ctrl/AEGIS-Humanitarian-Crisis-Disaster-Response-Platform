using CrisisPlatform.Api.Common.Security.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CrisisPlatform.Tests.Security;

public class LogRedactionTests
{
    private sealed class CapturingProvider : ILoggerProvider
    {
        public List<string> Messages { get; } = [];

        public ILogger CreateLogger(string categoryName) => new Capture(Messages);

        public void Dispose()
        {
        }

        private sealed class Capture(List<string> messages) : ILogger
        {
            public IDisposable? BeginScope<TState>(TState state) where TState : notnull
            {
                messages.Add("SCOPE:" + state);
                return null;
            }

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
                Func<TState, Exception?, string> formatter)
            {
                messages.Add(formatter(state, exception) + (exception is null ? "" : "|EX:" + exception.Message));
            }
        }
    }

    private const string Nic = "853202345V";
    private const string NewNic = "198532002345";
    private const string Email = "kamal.perera@example.org";
    private const string Phone = "0771234567";
    private const string Jwt = "eyJhbGciOiJIUzI1NiJ9.eyJzdWIiOiJ1c2VyLTEifQ.c2lnbmF0dXJlMTIz";

    private static (ILogger Logger, CapturingProvider Sink) Create()
    {
        var sink = new CapturingProvider();
        var services = new ServiceCollection().AddLogging(b =>
        {
            b.SetMinimumLevel(LogLevel.Trace);
            b.AddProvider(sink);
            b.AddRedaction();
        }).BuildServiceProvider();

        return (services.GetRequiredService<ILoggerFactory>().CreateLogger("test"), sink);
    }

    [Theory]
    [InlineData("Login failed for kamal.perera@example.org", Email)]
    [InlineData("Registering NIC 853202345V now", Nic)]
    [InlineData("Registering 198532002345 now", NewNic)]
    [InlineData("SMS to 0771234567 failed", Phone)]
    [InlineData("SMS to +94 77 123 4567 failed", "123 4567")]
    [InlineData("Auth header was Bearer abc.DEF-123_xyz", "abc.DEF-123_xyz")]
    [InlineData("Token eyJhbGciOiJIUzI1NiJ9.eyJzdWIiOiJ1c2VyLTEifQ.c2lnbmF0dXJlMTIz leaked", Jwt)]
    [InlineData("password=Sup3r-Secret!", "Sup3r-Secret!")]
    [InlineData("OTP: 482913", "482913")]
    [InlineData("Server=db;User Id=sa;Password=hunter2;Database=crisis", "hunter2")]
    [InlineData("signingKey: abcdef0123456789abcdef0123456789", "abcdef0123456789")]
    public void Sensitive_values_are_removed_from_text(string text, string sensitive)
    {
        var redacted = LogRedactor.Redact(text);

        Assert.DoesNotContain(sensitive, redacted);
        Assert.Contains(LogRedactor.Replacement, redacted);
    }

    [Fact]
    public void Ordinary_text_is_left_unchanged()
    {
        const string text = "Shelter capacity updated to 120 beds at 14:30";

        Assert.Equal(text, LogRedactor.Redact(text));
    }

    [Fact]
    public void Context_around_the_secret_is_kept_so_logs_stay_useful()
    {
        var redacted = LogRedactor.Redact("Login failed for kamal.perera@example.org from 10.0.0.5");

        Assert.StartsWith("Login failed for", redacted);
        Assert.EndsWith("from 10.0.0.5", redacted);
    }

    [Fact]
    public void Structured_arguments_are_redacted_in_the_final_message()
    {
        var (logger, sink) = Create();

        logger.LogInformation("User {Email} with NIC {Nic} called from {Phone}", Email, Nic, Phone);

        var message = Assert.Single(sink.Messages);
        Assert.DoesNotContain(Email, message);
        Assert.DoesNotContain(Nic, message);
        Assert.DoesNotContain(Phone, message);
    }

    [Fact]
    public void Exception_messages_and_stack_traces_are_redacted()
    {
        var (logger, sink) = Create();

        logger.LogError(new InvalidOperationException($"Duplicate NIC {NewNic} for {Email}"), "Registration failed");

        var combined = string.Join('\n', sink.Messages);
        Assert.DoesNotContain(NewNic, combined);
        Assert.DoesNotContain(Email, combined);
        Assert.Contains("Registration failed", combined);
    }

    [Fact]
    public void Scopes_are_not_forwarded_because_they_may_hold_personal_data()
    {
        var (logger, sink) = Create();

        using (logger.BeginScope(new Dictionary<string, object> { ["Email"] = Email }))
        {
            logger.LogInformation("Processing request");
        }

        Assert.DoesNotContain(sink.Messages, m => m.Contains(Email));
        Assert.DoesNotContain(sink.Messages, m => m.StartsWith("SCOPE:"));
    }

    [Fact]
    public void Every_log_level_goes_through_the_redactor()
    {
        var (logger, sink) = Create();

        foreach (var level in new[] { LogLevel.Trace, LogLevel.Debug, LogLevel.Information, LogLevel.Warning, LogLevel.Error, LogLevel.Critical })
        {
            logger.Log(level, "otp={Otp} for {Email}", "123456", Email);
        }

        Assert.Equal(6, sink.Messages.Count);
        Assert.All(sink.Messages, m =>
        {
            Assert.DoesNotContain("123456", m);
            Assert.DoesNotContain(Email, m);
        });
    }
}
