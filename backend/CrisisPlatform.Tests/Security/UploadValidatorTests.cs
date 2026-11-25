using System.Text;
using CrisisPlatform.Api.Common.Security.Uploads;

namespace CrisisPlatform.Tests.Security;

public class UploadValidatorTests
{
    private static readonly byte[] Pdf = [.. Encoding.ASCII.GetBytes("%PDF-1.7\n"), .. new byte[100]];
    private static readonly byte[] Png = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, .. new byte[100]];
    private static readonly byte[] Jpeg = [0xFF, 0xD8, 0xFF, 0xE0, .. new byte[100]];
    private static readonly byte[] Exe = [.. "MZ"u8.ToArray(), .. new byte[100]];

    private static UploadValidator Create(long max = 1024) => new(new UploadOptions { MaxBytes = max });

    private static Task<UploadValidationResult> Validate(
        string? name, string? type, byte[] bytes, long? length = null, long max = 1024)
        => Create(max).ValidateAsync(name, type, length ?? bytes.Length, new MemoryStream(bytes));

    [Theory]
    [InlineData("form.pdf", "application/pdf")]
    [InlineData("photo.png", "image/png")]
    [InlineData("photo.jpg", "image/jpeg")]
    [InlineData("photo.jpeg", "image/jpeg")]
    [InlineData("PHOTO.JPG", "IMAGE/JPEG")]
    [InlineData("form.pdf", "application/pdf; charset=binary")]
    public async Task Genuine_files_with_matching_type_are_accepted(string name, string type)
    {
        var bytes = name.EndsWith("pdf", StringComparison.OrdinalIgnoreCase) ? Pdf
            : name.EndsWith("png", StringComparison.OrdinalIgnoreCase) ? Png : Jpeg;

        var result = await Validate(name, type, bytes);

        Assert.True(result.IsValid, result.Error);
        Assert.Equal(Path.GetExtension(name).ToLowerInvariant(), result.Extension);
    }

    [Theory]
    [InlineData("tool.exe", "application/octet-stream")]
    [InlineData("page.html", "text/html")]
    [InlineData("script.js", "text/javascript")]
    [InlineData("vector.svg", "image/svg+xml")]
    [InlineData("doc.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document")]
    [InlineData("noextension", "application/pdf")]
    public async Task Disallowed_types_are_rejected(string name, string type)
    {
        var result = await Validate(name, type, Pdf);

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task Executable_renamed_to_pdf_is_rejected_by_its_signature()
    {
        var result = await Validate("invoice.pdf", "application/pdf", Exe);

        Assert.False(result.IsValid);
        Assert.Contains("content", result.Error);
    }

    [Fact]
    public async Task Png_content_with_a_jpg_name_is_rejected()
    {
        Assert.False((await Validate("photo.jpg", "image/jpeg", Png)).IsValid);
    }

    [Fact]
    public async Task Declared_content_type_must_match_the_extension()
    {
        var result = await Validate("form.pdf", "image/png", Pdf);

        Assert.False(result.IsValid);
        Assert.Contains("content type", result.Error);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task Missing_content_type_is_rejected(string? type)
    {
        Assert.False((await Validate("form.pdf", type, Pdf)).IsValid);
    }

    [Fact]
    public async Task File_at_the_size_limit_is_accepted_and_one_byte_over_is_rejected()
    {
        var atLimit = await Validate("form.pdf", "application/pdf", Pdf, length: 1024);
        var over = await Validate("form.pdf", "application/pdf", Pdf, length: 1025);

        Assert.True(atLimit.IsValid);
        Assert.False(over.IsValid);
        Assert.Contains("larger", over.Error);
    }

    [Fact]
    public async Task Size_is_judged_by_the_reported_length_so_a_lying_small_body_cannot_slip_through()
    {
        var result = await Validate("form.pdf", "application/pdf", Pdf, length: 10_000_000);

        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Empty_files_are_rejected(long length)
    {
        Assert.False((await Validate("form.pdf", "application/pdf", Pdf, length: length)).IsValid);
    }

    [Fact]
    public async Task File_shorter_than_its_signature_is_rejected()
    {
        Assert.False((await Validate("form.png", "image/png", [0x89, 0x50])).IsValid);
    }

    [Theory]
    [InlineData("../../etc/passwd.pdf")]
    [InlineData("..\\windows\\evil.pdf")]
    [InlineData("a/b.pdf")]
    [InlineData("C:evil.pdf")]
    [InlineData("name\0.pdf")]
    [InlineData("na\nme.pdf")]
    [InlineData("report.php.pdf")]
    [InlineData("shell.aspx.png")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task Dangerous_file_names_are_rejected(string? name)
    {
        Assert.False((await Validate(name, "application/pdf", Pdf)).IsValid);
    }

    [Fact]
    public async Task Overlong_file_name_is_rejected()
    {
        Assert.False((await Validate(new string('a', 252) + ".pdf", "application/pdf", Pdf)).IsValid);
    }

    [Fact]
    public void Stored_name_never_contains_the_client_name_and_keeps_only_the_validated_extension()
    {
        var stored = UploadValidator.CreateStoredName(".pdf");

        Assert.EndsWith(".pdf", stored);
        Assert.Equal(32 + 4, stored.Length);
        Assert.NotEqual(stored, UploadValidator.CreateStoredName(".pdf"));
    }
}
