using System.Text.RegularExpressions;

namespace CrisisPlatform.Api.Modules.AI;

public static partial class PiiRedactor
{
    public static string Redact(string? input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;
        var text = OldNicRegex().Replace(input, "[REDACTED_NIC]");
        text = NewNicRegex().Replace(text, "[REDACTED_NIC]");
        text = PhoneRegex().Replace(text, "[REDACTED_PHONE]");
        text = EmailRegex().Replace(text, "[REDACTED_EMAIL]");
        return text;
    }

    [GeneratedRegex(@"\b\d{9}[vVxX]\b")]
    private static partial Regex OldNicRegex();

    [GeneratedRegex(@"\b\d{12}\b")]
    private static partial Regex NewNicRegex();

    [GeneratedRegex(@"(?:\+94|0)7\d{8}\b")]
    private static partial Regex PhoneRegex();

    [GeneratedRegex(@"[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}")]
    private static partial Regex EmailRegex();
}
