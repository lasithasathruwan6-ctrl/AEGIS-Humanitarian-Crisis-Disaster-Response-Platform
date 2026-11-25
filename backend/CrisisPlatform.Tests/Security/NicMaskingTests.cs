using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using CrisisPlatform.Api.Common.Security.Nic;

namespace CrisisPlatform.Tests.Security;

public class NicMaskingTests
{
    private sealed record ProfileResponse(
        string FullName,
        [property: JsonConverter(typeof(MaskedNicJsonConverter))] string? Nic);

    [Theory]
    [InlineData("853202345V", "*******45V")]
    [InlineData("853202345x", "*******45X")]
    [InlineData("198532002345", "*********345")]
    [InlineData("ABC", "***")]
    [InlineData("AB", "**")]
    public void Only_the_last_three_characters_stay_visible(string nic, string expected)
    {
        Assert.Equal(expected, NicMasker.Mask(nic));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Missing_nic_masks_to_empty_string(string? nic)
    {
        Assert.Equal(string.Empty, NicMasker.Mask(nic));
    }

    [Fact]
    public void Masked_value_never_reveals_the_leading_digits()
    {
        var masked = NicMasker.Mask("198532002345");

        Assert.DoesNotContain("1985", masked);
        Assert.Equal(12, masked.Length);
    }

    [Fact]
    public void Serialised_response_contains_only_the_masked_nic()
    {
        var json = JsonSerializer.Serialize(new ProfileResponse("Kamal Perera", "853202345V"));

        Assert.DoesNotContain("853202345", json);
        Assert.Contains("*******45V", json);
        Assert.Contains("Kamal Perera", json);
    }

    [Fact]
    public async Task Api_response_over_http_exposes_only_the_masked_nic()
    {
        var client = await TestHostFactory.CreateClientAsync(null, app =>
            app.MapGet("/profile", () => new ProfileResponse("Kamal Perera", "198532002345")));

        var body = await client.GetStringAsync("/profile");
        var profile = await client.GetFromJsonAsync<ProfileResponse>("/profile");

        Assert.DoesNotContain("198532002345", body);
        Assert.Equal("*********345", profile!.Nic);
    }
}
