using System.Text.RegularExpressions;

namespace CrisisPlatform.Api.Modules.Requests;

/// <summary>Represents a field-level validation error.</summary>
public sealed record ValidationFailure(string PropertyName, string ErrorMessage);

/// <summary>Result of validating a DTO.</summary>
public sealed record ValidationOutcome(bool IsValid, IReadOnlyList<ValidationFailure> Errors)
{
    public static ValidationOutcome Success() => new(true, []);
    public static ValidationOutcome Failure(IReadOnlyList<ValidationFailure> errors) => new(false, errors);

    public IDictionary<string, string[]> ToDictionary() =>
        Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(x => x.ErrorMessage).ToArray());
}

/// <summary>Validates <see cref="CreateRequestDto"/> payloads before business processing.</summary>
public sealed partial class CreateRequestValidator
{
    private static readonly IReadOnlySet<string> ValidNeedTypes =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Food", "Water", "Medical", "Shelter", "Evacuation", "Other"
        };

    [GeneratedRegex(@"^(\+94|0)?7\d{8}$")]
    private static partial Regex SriLankaPhoneRegex();

    public ValidationOutcome Validate(CreateRequestDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        var errors = new List<ValidationFailure>();

        if (string.IsNullOrWhiteSpace(dto.NicHash) || dto.NicHash.Length < 16)
            errors.Add(new(nameof(dto.NicHash), "NicHash must be at least 16 characters."));

        if (string.IsNullOrWhiteSpace(dto.District) || dto.District.Trim().Length is < 2 or > 80)
            errors.Add(new(nameof(dto.District), "District must be between 2 and 80 characters."));

        if (string.IsNullOrWhiteSpace(dto.NeedType) || !ValidNeedTypes.Contains(dto.NeedType))
            errors.Add(new(nameof(dto.NeedType), "NeedType must be one of: Food, Water, Medical, Shelter, Evacuation, Other."));

        if (dto.Urgency is < 1 or > 5)
            errors.Add(new(nameof(dto.Urgency), "Urgency must be between 1 and 5."));

        if (string.IsNullOrWhiteSpace(dto.Description) || dto.Description.Trim().Length is < 10 or > 1000)
            errors.Add(new(nameof(dto.Description), "Description must be between 10 and 1000 characters."));

        if (string.IsNullOrWhiteSpace(dto.ContactPhone) || !SriLankaPhoneRegex().IsMatch(dto.ContactPhone.Trim()))
            errors.Add(new(nameof(dto.ContactPhone), "ContactPhone must be a valid Sri Lankan mobile number (e.g. 0771234567 or +94771234567)."));

        return errors.Count == 0
            ? ValidationOutcome.Success()
            : ValidationOutcome.Failure(errors);
    }
}
