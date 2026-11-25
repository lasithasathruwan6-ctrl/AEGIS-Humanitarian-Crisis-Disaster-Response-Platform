using System.Text;

namespace CrisisPlatform.Api.Common.Security.Uploads;

public sealed class UploadOptions
{
    public const string SectionName = "Uploads";

    public long MaxBytes { get; set; } = 5 * 1024 * 1024;
}

public sealed record UploadValidationResult(bool IsValid, string? Error, string? Extension = null, string? CanonicalContentType = null)
{
    public static UploadValidationResult Fail(string error) => new(false, error);
}

/// <summary>
/// Accepts only PDF, JPEG and PNG files, and trusts none of what the client says: the extension, the declared
/// content type and the leading bytes of the file must all agree.
/// </summary>
public sealed class UploadValidator(UploadOptions options)
{
    private sealed record FileKind(string ContentType, byte[][] Signatures);

    private static readonly Dictionary<string, FileKind> Kinds = new(StringComparer.OrdinalIgnoreCase)
    {
        [".pdf"] = new("application/pdf", [Encoding.ASCII.GetBytes("%PDF-")]),
        [".png"] = new("image/png", [[0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]]),
        [".jpg"] = new("image/jpeg", [[0xFF, 0xD8, 0xFF]]),
        [".jpeg"] = new("image/jpeg", [[0xFF, 0xD8, 0xFF]]),
    };

    private static readonly HashSet<string> ExecutableExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".exe", ".dll", ".bat", ".cmd", ".com", ".msi", ".ps1", ".sh", ".js", ".vbs", ".jar",
        ".php", ".asp", ".aspx", ".cshtml", ".html", ".htm", ".svg", ".config",
    };

    public async Task<UploadValidationResult> ValidateAsync(
        string? fileName, string? contentType, long length, Stream content, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(fileName)
            || fileName.Length > 255
            || fileName.IndexOfAny(['/', '\\', '\0', ':']) >= 0
            || fileName.Any(char.IsControl)
            || fileName.Contains(".."))
        {
            return UploadValidationResult.Fail("File name is not allowed.");
        }

        var extension = Path.GetExtension(fileName);
        if (!Kinds.TryGetValue(extension, out var kind))
        {
            return UploadValidationResult.Fail("Only PDF, JPEG and PNG files are accepted.");
        }

        // "report.php.pdf" and similar tricks: no executable extension may appear anywhere in the name.
        var parts = Path.GetFileNameWithoutExtension(fileName).Split('.');
        if (parts.Skip(1).Any(p => ExecutableExtensions.Contains("." + p)))
        {
            return UploadValidationResult.Fail("File name is not allowed.");
        }

        if (length <= 0)
        {
            return UploadValidationResult.Fail("File is empty.");
        }

        if (length > options.MaxBytes)
        {
            return UploadValidationResult.Fail($"File is larger than {options.MaxBytes} bytes.");
        }

        if (!string.Equals(contentType?.Split(';')[0].Trim(), kind.ContentType, StringComparison.OrdinalIgnoreCase))
        {
            return UploadValidationResult.Fail("Declared content type does not match the file extension.");
        }

        var header = new byte[8];
        var read = 0;
        while (read < header.Length)
        {
            var n = await content.ReadAsync(header.AsMemory(read), cancellationToken);
            if (n == 0)
            {
                break;
            }

            read += n;
        }

        if (!kind.Signatures.Any(sig => read >= sig.Length && header.AsSpan(0, sig.Length).SequenceEqual(sig)))
        {
            return UploadValidationResult.Fail("File content does not match its type.");
        }

        return new UploadValidationResult(true, null, extension.ToLowerInvariant(), kind.ContentType);
    }

    /// <summary>A server-generated storage name; the client's file name is never used on disk.</summary>
    public static string CreateStoredName(string validatedExtension) => $"{Guid.NewGuid():N}{validatedExtension}";
}
