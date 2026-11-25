using CrisisPlatform.Api.Modules.Auth.Tokens;
using Microsoft.IdentityModel.JsonWebTokens;

namespace CrisisPlatform.Tests.Security;

public class AccessTokenTests
{
    private static TokenOptions Options(int minutes = 15) => new()
    {
        SigningKey = "unit-test-signing-key-0123456789-abcdef",
        AccessTokenMinutes = minutes,
    };

    private static readonly TimeProvider RealClock = TimeProvider.System;

    [Fact]
    public async Task Fresh_token_is_valid_and_carries_subject_and_role()
    {
        var service = new AccessTokenService(Options(), RealClock);

        var result = await service.ValidateAsync(service.Create("user-1", "Citizen"));

        Assert.True(result.IsValid);
        Assert.Equal("user-1", result.ClaimsIdentity.FindFirst("sub")!.Value);
        Assert.Equal("Citizen", result.Claims["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"]);
    }

    [Fact]
    public void Access_token_lifetime_is_exactly_the_configured_minutes()
    {
        var clock = new ManualTimeProvider(new DateTimeOffset(2026, 3, 1, 9, 0, 0, TimeSpan.Zero));
        var service = new AccessTokenService(Options(15), clock);

        var jwt = new JsonWebToken(service.Create("user-1", "Citizen"));

        Assert.Equal(TimeSpan.FromMinutes(15), jwt.ValidTo - jwt.ValidFrom);
    }

    [Fact]
    public async Task Expired_token_is_rejected_without_clock_skew()
    {
        var past = new ManualTimeProvider(DateTimeOffset.UtcNow.AddMinutes(-16));
        var token = new AccessTokenService(Options(15), past).Create("user-1", "Citizen");

        var result = await new AccessTokenService(Options(15), RealClock).ValidateAsync(token);

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task Token_signed_with_another_key_is_rejected()
    {
        var other = new TokenOptions { SigningKey = "a-completely-different-signing-key-9876543210" };
        var forged = new AccessTokenService(other, RealClock).Create("user-1", "Admin");

        var result = await new AccessTokenService(Options(), RealClock).ValidateAsync(forged);

        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(31)]
    [InlineData(600)]
    public void Excessive_or_zero_lifetime_is_refused_at_startup(int minutes)
    {
        Assert.Throws<InvalidOperationException>(() => new AccessTokenService(Options(minutes), RealClock));
    }

    [Fact]
    public void Short_signing_key_is_refused_at_startup()
    {
        var options = new TokenOptions { SigningKey = "too-short" };

        Assert.Throws<InvalidOperationException>(() => new AccessTokenService(options, RealClock));
    }
}
