using CrisisPlatform.Api.Common.Security.Nic;

namespace CrisisPlatform.Tests.Security;

public class NicValidatorTests
{
    [Theory]
    [InlineData("853202345V")]
    [InlineData("853202345X")]
    [InlineData("853202345v")]
    [InlineData("  853202345x  ")]
    [InlineData("856202345V")] // female: 500 added to day-of-year
    public void Old_format_is_accepted(string nic)
    {
        var result = NicValidator.Validate(nic);

        Assert.True(result.IsValid);
        Assert.Equal(NicFormat.Old, result.Format);
        Assert.Equal(nic.Trim().ToUpperInvariant(), result.Normalised);
    }

    [Theory]
    [InlineData("198532002345")]
    [InlineData("200012345678")]
    [InlineData("199586012345")] // female
    public void New_format_is_accepted(string nic)
    {
        var result = NicValidator.Validate(nic);

        Assert.True(result.IsValid);
        Assert.Equal(NicFormat.New, result.Format);
        Assert.Equal(nic, result.Normalised);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("85320234V")]        // 8 digits
    [InlineData("8532023456V")]      // 10 digits
    [InlineData("853202345A")]       // wrong suffix
    [InlineData("853202345VV")]      // extra letter
    [InlineData("85320234512")]      // 11 digits
    [InlineData("1985320023456")]    // 13 digits
    [InlineData("185320023456")]     // century not 19/20
    [InlineData("19853200234X")]     // letter inside new format
    [InlineData("85-3202345V")]      // punctuation
    [InlineData("853202345 V")]      // inner space
    public void Wrong_shape_is_rejected(string? nic)
    {
        var result = NicValidator.Validate(nic);

        Assert.False(result.IsValid);
        Assert.Null(result.Normalised);
        Assert.NotNull(result.Error);
    }

    [Theory]
    [InlineData("850002345V")] // day 000
    [InlineData("853672345V")] // day 367
    [InlineData("854992345V")] // day 499
    [InlineData("858672345V")] // day 867
    [InlineData("198500002345")]
    [InlineData("198536702345")]
    public void Impossible_birth_day_is_rejected(string nic)
    {
        var result = NicValidator.Validate(nic);

        Assert.False(result.IsValid);
        Assert.Contains("birth day", result.Error);
    }

    [Theory]
    [InlineData("850012345V")] // first day
    [InlineData("853662345V")] // last day, male
    [InlineData("855012345V")] // first day, female
    [InlineData("858662345V")] // last day, female
    public void Boundary_days_are_accepted(string nic)
    {
        Assert.True(NicValidator.Validate(nic).IsValid);
    }
}
