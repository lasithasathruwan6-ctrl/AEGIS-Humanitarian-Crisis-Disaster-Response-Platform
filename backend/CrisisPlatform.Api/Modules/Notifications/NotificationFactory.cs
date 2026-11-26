using CrisisPlatform.Api.Modules.Requests;

namespace CrisisPlatform.Api.Modules.Notifications;

/// <summary>Supported notification locales.</summary>
public enum NotificationLocale
{
    English,
    Sinhala,
    Tamil,
}

/// <summary>Supported notification event types.</summary>
public enum NotificationTemplate
{
    RequestReceived,
    AidDispatched,
    RequestFulfilled,
}

/// <summary>Input parameters for <see cref="NotificationFactory"/>.</summary>
public sealed record NotificationInput(
    NotificationTemplate Template,
    string               Locale,
    string               ReferenceCode,
    string               District);

/// <summary>Constructed notification message ready for delivery.</summary>
public sealed record NotificationMessage(
    string Locale,
    string Subject,
    string Body);

/// <summary>
/// <b>Pattern:</b> Factory Method (GoF). Selects the localised subject and body
/// template for English (<c>en</c>), Sinhala (<c>si</c>), or Tamil (<c>ta</c>)
/// without exposing locale-branching logic to callers.
/// </summary>
public sealed class NotificationFactory : IFactory<NotificationInput, NotificationMessage>
{
    public NotificationMessage Create(NotificationInput input)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(input.ReferenceCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(input.District);

        var locale = NormalizeLocale(input.Locale);

        return (locale, input.Template) switch
        {
            ("si", NotificationTemplate.RequestReceived) => new(
                "si",
                $"ආධාර ඉල්ලීම ලැබුණි ({input.ReferenceCode})",
                $"{input.District} දිස්ත්‍රික්කය සඳහා ඔබගේ ආධාර ඉල්ලීම ({input.ReferenceCode}) ලියාපදිංචි කර ඇත."),

            ("si", NotificationTemplate.AidDispatched) => new(
                "si",
                $"ආධාර පිටත් කර ඇත ({input.ReferenceCode})",
                $"{input.District} දිස්ත්‍රික්කය වෙත ඔබගේ ආධාර ඉල්ලීම ({input.ReferenceCode}) සඳහා සැපයුම් පිටත් කර ඇත."),

            ("si", NotificationTemplate.RequestFulfilled) => new(
                "si",
                $"ආධාර ඉල්ලීම සම්පූර්ණයි ({input.ReferenceCode})",
                $"{input.District} දිස්ත්‍රික්කයේ ඔබගේ ආධාර ඉල්ලීම ({input.ReferenceCode}) සම්පූර්ණ කර ඇත."),

            ("ta", NotificationTemplate.RequestReceived) => new(
                "ta",
                $"உதவி கோரிக்கை பெறப்பட்டது ({input.ReferenceCode})",
                $"{input.District} மாவட்டத்திற்கான உங்கள் உதவி கோரிக்கை ({input.ReferenceCode}) பதிவு செய்யப்பட்டது."),

            ("ta", NotificationTemplate.AidDispatched) => new(
                "ta",
                $"உதவி அனுப்பப்பட்டது ({input.ReferenceCode})",
                $"{input.District} மாவட்டத்திற்கு உங்கள் கோரிக்கைக்காக ({input.ReferenceCode}) பொருட்கள் அனுப்பப்பட்டுள்ளன."),

            ("ta", NotificationTemplate.RequestFulfilled) => new(
                "ta",
                $"உதவி கோரிக்கை நிறைவடைந்தது ({input.ReferenceCode})",
                $"{input.District} மாவட்டத்தில் உங்கள் உதவி கோரிக்கை ({input.ReferenceCode}) நிறைவு செய்யப்பட்டது."),

            (_, NotificationTemplate.RequestReceived) => new(
                "en",
                $"Aid request received ({input.ReferenceCode})",
                $"Your aid request ({input.ReferenceCode}) for {input.District} district has been registered."),

            (_, NotificationTemplate.AidDispatched) => new(
                "en",
                $"Aid dispatched ({input.ReferenceCode})",
                $"Supplies for your aid request ({input.ReferenceCode}) in {input.District} district have been dispatched."),

            (_, NotificationTemplate.RequestFulfilled) => new(
                "en",
                $"Aid request fulfilled ({input.ReferenceCode})",
                $"Your aid request ({input.ReferenceCode}) in {input.District} district has been fulfilled."),

            _ => throw new ArgumentOutOfRangeException(nameof(input.Template), input.Template, "Unsupported notification template."),
        };
    }

    private static string NormalizeLocale(string? locale) =>
        locale?.Trim().ToLowerInvariant() switch
        {
            "si" or "si-lk" => "si",
            "ta" or "ta-lk" => "ta",
            _               => "en",
        };
}
