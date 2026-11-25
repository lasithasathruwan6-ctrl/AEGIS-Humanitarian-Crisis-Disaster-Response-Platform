namespace CrisisPlatform.Api.Common.Security;

/// <summary>Server-side password rules applied at registration and password change.</summary>
public static class PasswordPolicy
{
    public const int MinLength = 12;
    public const int MaxLength = 128;

    private static readonly HashSet<string> CommonPasswords = new(StringComparer.OrdinalIgnoreCase)
    {
        "password1234", "passw0rd1234", "123456789012", "qwerty123456",
        "letmein12345", "welcome12345", "admin1234567", "iloveyou1234",
    };

    /// <summary>Returns the list of violated rules; an empty list means the password is acceptable.</summary>
    public static IReadOnlyList<string> Validate(string? password, string? email = null)
    {
        var errors = new List<string>();

        if (string.IsNullOrEmpty(password))
        {
            errors.Add("Password is required.");
            return errors;
        }

        if (password.Length < MinLength)
        {
            errors.Add($"Password must be at least {MinLength} characters.");
        }

        if (password.Length > MaxLength)
        {
            errors.Add($"Password must be at most {MaxLength} characters.");
        }

        if (!password.Any(char.IsUpper))
        {
            errors.Add("Password must contain an uppercase letter.");
        }

        if (!password.Any(char.IsLower))
        {
            errors.Add("Password must contain a lowercase letter.");
        }

        if (!password.Any(char.IsDigit))
        {
            errors.Add("Password must contain a digit.");
        }

        if (!password.Any(c => !char.IsLetterOrDigit(c) && !char.IsWhiteSpace(c)))
        {
            errors.Add("Password must contain a symbol.");
        }

        if (CommonPasswords.Contains(password))
        {
            errors.Add("Password is too common.");
        }

        var localPart = email?.Split('@')[0];
        if (!string.IsNullOrWhiteSpace(localPart) && localPart.Length >= 4
            && password.Contains(localPart, StringComparison.OrdinalIgnoreCase))
        {
            errors.Add("Password must not contain your email name.");
        }

        return errors;
    }

    public static bool IsValid(string? password, string? email = null) => Validate(password, email).Count == 0;
}
