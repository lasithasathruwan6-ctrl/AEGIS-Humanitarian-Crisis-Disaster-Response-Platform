using CrisisPlatform.Api.Common.Security;

namespace CrisisPlatform.Tests.Security;

public class PasswordPolicyTests
{
    private const string Strong = "Tr4nquil-Harbour!";

    [Fact]
    public void Strong_password_is_accepted()
    {
        Assert.Empty(PasswordPolicy.Validate(Strong));
        Assert.True(PasswordPolicy.IsValid(Strong));
    }

    [Theory]
    [InlineData(null, "required")]
    [InlineData("", "required")]
    [InlineData("Ab1!short", "at least 12")]
    [InlineData("NOLOWERCASE1234!", "lowercase")]
    [InlineData("nouppercase1234!", "uppercase")]
    [InlineData("NoDigitsHere-Now!", "digit")]
    [InlineData("NoSymbolsHere1234", "symbol")]
    public void Weak_passwords_are_rejected_with_the_matching_rule(string? password, string expectedFragment)
    {
        var errors = PasswordPolicy.Validate(password);

        Assert.Contains(errors, e => e.Contains(expectedFragment, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Password_longer_than_the_maximum_is_rejected()
    {
        var password = Strong + new string('x', PasswordPolicy.MaxLength);

        Assert.Contains(PasswordPolicy.Validate(password), e => e.Contains("at most"));
    }

    [Fact]
    public void Exactly_minimum_length_is_accepted()
    {
        var password = "Aa1!" + new string('b', PasswordPolicy.MinLength - 4);

        Assert.Equal(PasswordPolicy.MinLength, password.Length);
        Assert.True(PasswordPolicy.IsValid(password));
    }

    [Fact]
    public void Common_passwords_are_rejected_even_when_they_satisfy_other_rules()
    {
        Assert.Contains(PasswordPolicy.Validate("password1234"), e => e.Contains("common"));
    }

    [Fact]
    public void Password_containing_the_email_name_is_rejected()
    {
        var errors = PasswordPolicy.Validate("Kamal.Perera-2026!", "kamal.perera@example.org");

        Assert.Contains(errors, e => e.Contains("email"));
    }

    [Fact]
    public void Several_violations_are_reported_together()
    {
        Assert.True(PasswordPolicy.Validate("abc").Count >= 4);
    }
}
