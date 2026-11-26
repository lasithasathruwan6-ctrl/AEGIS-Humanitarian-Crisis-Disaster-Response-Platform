using CrisisPlatform.Api.Modules.Requests;

namespace CrisisPlatform.Tests.Requests;

/// <summary>Unit tests for <see cref="CreateRequestValidator"/>.</summary>
public sealed class CreateRequestValidatorTests
{
    private static readonly CreateRequestValidator Sut = new();

    private static CreateRequestDto ValidDto() => new(
        NicHash:      "a1b2c3d4e5f6g7h8i9j0",
        District:     "Colombo",
        NeedType:     "Water",
        Urgency:      4,
        Description:  "Drinking water needed urgently for 12 people.",
        ContactPhone: "0771234567");

    [Fact]
    public void Validate_Passes_ForValidDto()
    {
        var result = Sut.Validate(ValidDto());
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    [InlineData(-1)]
    public void Validate_Fails_WhenUrgencyOutOfRange(int urgency)
    {
        var result = Sut.Validate(ValidDto() with { Urgency = urgency });
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateRequestDto.Urgency));
    }

    [Theory]
    [InlineData("Weapons")]
    [InlineData("")]
    public void Validate_Fails_WhenNeedTypeInvalid(string needType)
    {
        var result = Sut.Validate(ValidDto() with { NeedType = needType });
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateRequestDto.NeedType));
    }

    [Theory]
    [InlineData("12345")]
    [InlineData("0112345678")]
    [InlineData("")]
    public void Validate_Fails_WhenContactPhoneInvalid(string phone)
    {
        var result = Sut.Validate(ValidDto() with { ContactPhone = phone });
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateRequestDto.ContactPhone));
    }

    [Fact]
    public void Validate_AcceptsInternationalSriLankaPhoneFormat()
    {
        var result = Sut.Validate(ValidDto() with { ContactPhone = "+94771234567" });
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_Fails_WhenDescriptionTooShortOrNicHashMissing()
    {
        var result = Sut.Validate(ValidDto() with { Description = "short", NicHash = "abc" });
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateRequestDto.Description));
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateRequestDto.NicHash));
    }
}
