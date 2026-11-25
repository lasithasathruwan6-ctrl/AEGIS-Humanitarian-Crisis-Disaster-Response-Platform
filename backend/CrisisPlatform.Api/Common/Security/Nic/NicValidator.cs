using System.Text.RegularExpressions;

namespace CrisisPlatform.Api.Common.Security.Nic;

public enum NicFormat
{
    Unknown,
    Old,
    New,
}

public sealed record NicValidationResult(bool IsValid, string? Normalised, NicFormat Format, string? Error)
{
    public static NicValidationResult Fail(string error) => new(false, null, NicFormat.Unknown, error);
}

/// <summary>
/// Validates Sri Lankan National Identity Card numbers: the old format (9 digits + V or X)
/// and the new format (12 digits). The embedded day-of-year must be plausible.
/// </summary>
public static partial class NicValidator
{
    [GeneratedRegex(@"^(?<year>\d{2})(?<day>\d{3})\d{3}(?<check>\d)[VX]$")]
    private static partial Regex OldFormat();

    [GeneratedRegex(@"^(?<year>(19|20)\d{2})(?<day>\d{3})\d{5}$")]
    private static partial Regex NewFormat();

    public static NicValidationResult Validate(string? nic)
    {
        if (string.IsNullOrWhiteSpace(nic))
        {
            return NicValidationResult.Fail("NIC is required.");
        }

        var value = nic.Trim().ToUpperInvariant();

        var old = OldFormat().Match(value);
        if (old.Success)
        {
            return IsPlausibleDay(int.Parse(old.Groups["day"].Value))
                ? new NicValidationResult(true, value, NicFormat.Old, null)
                : NicValidationResult.Fail("NIC contains an invalid birth day.");
        }

        var modern = NewFormat().Match(value);
        if (modern.Success)
        {
            return IsPlausibleDay(int.Parse(modern.Groups["day"].Value))
                ? new NicValidationResult(true, value, NicFormat.New, null)
                : NicValidationResult.Fail("NIC contains an invalid birth day.");
        }

        return NicValidationResult.Fail("NIC must be 9 digits followed by V or X, or 12 digits.");
    }

    /// <summary>Day of year 1-366 for men; women have 500 added (501-866).</summary>
    private static bool IsPlausibleDay(int day) => day is >= 1 and <= 366 or >= 501 and <= 866;
}
