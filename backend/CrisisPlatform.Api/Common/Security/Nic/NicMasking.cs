using System.Text.Json;
using System.Text.Json.Serialization;

namespace CrisisPlatform.Api.Common.Security.Nic;

public static class NicMasker
{
    private const int VisibleCharacters = 3;

    /// <summary>Hides all but the last three characters, e.g. 853202345V becomes *******45V.</summary>
    public static string Mask(string? nic)
    {
        var value = nic?.Trim().ToUpperInvariant();
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        return value.Length <= VisibleCharacters
            ? new string('*', value.Length)
            : new string('*', value.Length - VisibleCharacters) + value[^VisibleCharacters..];
    }
}

/// <summary>
/// Put on a response DTO property so the NIC can only ever leave the API masked,
/// even if a developer forgets to mask it by hand.
/// </summary>
public sealed class MaskedNicJsonConverter : JsonConverter<string?>
{
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => reader.GetString();

    public override void Write(Utf8JsonWriter writer, string? value, JsonSerializerOptions options)
        => writer.WriteStringValue(NicMasker.Mask(value));
}
