using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CrisisPlatform.Api.Common.Security.Logging;

/// <summary>Removes personal data and secrets from text before it is written to any log.</summary>
public static partial class LogRedactor
{
    public const string Replacement = "[REDACTED]";

    [GeneratedRegex(@"[A-Za-z0-9._%+\-]+@[A-Za-z0-9.\-]+\.[A-Za-z]{2,}")]
    private static partial Regex Email();

    [GeneratedRegex(@"(?<![0-9A-Za-z])(?:\d{9}[VvXx]|(?:19|20)\d{10})(?![0-9A-Za-z])")]
    private static partial Regex Nic();

    [GeneratedRegex(@"(?<!\d)(?:\+94|0)\s?7\d[\s-]?\d{3}[\s-]?\d{4}(?!\d)")]
    private static partial Regex Phone();

    [GeneratedRegex(@"eyJ[\w\-]+\.[\w\-]+\.[\w\-]*")]
    private static partial Regex Jwt();

    [GeneratedRegex(@"(?i)\b(bearer|basic)\s+[A-Za-z0-9\-._~+/]+=*")]
    private static partial Regex AuthScheme();

    [GeneratedRegex(@"(?i)\b(password|passwd|pwd|secret|token|otp|code|api[_\-]?key|signingkey|key|authorization|connectionstring|nic)\b(\s*[=:]\s*)(""[^""]*""|'[^']*'|[^\s;,&]+)")]
    private static partial Regex KeyValue();

    public static string Redact(string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return text ?? string.Empty;
        }

        text = Jwt().Replace(text, Replacement);
        text = AuthScheme().Replace(text, m => $"{m.Groups[1].Value} {Replacement}");
        text = KeyValue().Replace(text, m => $"{m.Groups[1].Value}{m.Groups[2].Value}{Replacement}");
        text = Email().Replace(text, Replacement);
        text = Nic().Replace(text, Replacement);
        text = Phone().Replace(text, Replacement);
        return text;
    }
}

internal sealed class RedactingLogger(ILogger inner) : ILogger
{
    // Scopes are dropped on purpose: scope state is arbitrary objects that could carry personal data.
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => inner.IsEnabled(logLevel);

    public void Log<TState>(
        LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        var message = formatter(state, null);
        if (exception is not null)
        {
            message += Environment.NewLine + exception;
        }

        inner.Log(logLevel, eventId, LogRedactor.Redact(message), null, static (text, _) => text);
    }
}

internal sealed class RedactingLoggerProvider(ILoggerProvider inner) : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName) => new RedactingLogger(inner.CreateLogger(categoryName));

    public void Dispose() => inner.Dispose();
}

public static class LogRedactionExtensions
{
    /// <summary>Wraps every logging provider registered so far so that all output passes through <see cref="LogRedactor"/>.</summary>
    public static ILoggingBuilder AddRedaction(this ILoggingBuilder builder)
    {
        var providers = builder.Services.Where(d => d.ServiceType == typeof(ILoggerProvider)).ToList();

        foreach (var descriptor in providers)
        {
            builder.Services.Remove(descriptor);
            builder.Services.AddSingleton<ILoggerProvider>(sp => new RedactingLoggerProvider(Create(sp, descriptor)));
        }

        return builder;
    }

    private static ILoggerProvider Create(IServiceProvider sp, ServiceDescriptor descriptor)
    {
        if (descriptor.ImplementationInstance is ILoggerProvider instance)
        {
            return instance;
        }

        if (descriptor.ImplementationFactory is not null)
        {
            return (ILoggerProvider)descriptor.ImplementationFactory(sp);
        }

        return (ILoggerProvider)ActivatorUtilities.CreateInstance(sp, descriptor.ImplementationType!);
    }
}
