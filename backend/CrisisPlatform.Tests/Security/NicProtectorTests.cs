using System.Security.Cryptography;
using CrisisPlatform.Api.Common.Security.Nic;

namespace CrisisPlatform.Tests.Security;

public class NicProtectorTests
{
    private static NicProtector Create() => new(RandomNumberGenerator.GetBytes(32));

    [Theory]
    [InlineData("853202345V")]
    [InlineData("853202345X")]
    [InlineData("198532002345")]
    public void Round_trip_returns_the_original_nic(string nic)
    {
        var protector = Create();

        Assert.Equal(nic, protector.Unprotect(protector.Protect(nic)));
    }

    [Fact]
    public void Lowercase_input_is_normalised_before_encryption()
    {
        var protector = Create();

        Assert.Equal("853202345V", protector.Unprotect(protector.Protect(" 853202345v ")));
    }

    [Fact]
    public void Stored_value_does_not_contain_the_plain_nic()
    {
        var stored = Create().Protect("198532002345");

        Assert.DoesNotContain("198532002345", stored);
        Assert.DoesNotContain("1985", Convert.ToHexString(Convert.FromBase64String(stored)));
    }

    [Fact]
    public void Same_nic_encrypts_differently_each_time_because_the_nonce_is_random()
    {
        var protector = Create();

        Assert.NotEqual(protector.Protect("853202345V"), protector.Protect("853202345V"));
    }

    [Fact]
    public void Flipping_any_single_bit_anywhere_in_the_stored_value_is_detected()
    {
        var protector = Create();
        var blob = Convert.FromBase64String(protector.Protect("853202345V"));

        for (var i = 0; i < blob.Length; i++)
        {
            var tampered = (byte[])blob.Clone();
            tampered[i] ^= 0x01;

            Assert.ThrowsAny<CryptographicException>(() => protector.Unprotect(Convert.ToBase64String(tampered)));
        }
    }

    [Fact]
    public void Value_encrypted_with_another_key_cannot_be_read()
    {
        var stored = Create().Protect("853202345V");

        Assert.ThrowsAny<CryptographicException>(() => Create().Unprotect(stored));
    }

    [Theory]
    [InlineData("not base64!!")]
    [InlineData("")]
    [InlineData("AAAA")]
    public void Malformed_values_are_rejected(string stored)
    {
        Assert.ThrowsAny<CryptographicException>(() => Create().Unprotect(stored));
    }

    [Fact]
    public void Invalid_nic_is_refused_instead_of_encrypted()
    {
        Assert.Throws<ArgumentException>(() => Create().Protect("12345"));
    }

    [Theory]
    [InlineData(16)]
    [InlineData(31)]
    [InlineData(33)]
    public void Key_must_be_exactly_256_bits(int length)
    {
        Assert.Throws<ArgumentException>(() => new NicProtector(new byte[length]));
    }
}
