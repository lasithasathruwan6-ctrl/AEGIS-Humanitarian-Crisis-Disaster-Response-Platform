using CrisisPlatform.Api.Modules.Notifications;

namespace CrisisPlatform.Tests.Notifications;

/// <summary>Unit tests for <see cref="NotificationFactory"/>.</summary>
public sealed class NotificationFactoryTests
{
    private static readonly NotificationFactory Sut = new();

    [Theory]
    [InlineData(NotificationTemplate.RequestReceived, "Aid request received")]
    [InlineData(NotificationTemplate.AidDispatched,   "Aid dispatched")]
    [InlineData(NotificationTemplate.RequestFulfilled,"Aid request fulfilled")]
    public void Create_BuildsEnglishMessage_ForEnLocale(NotificationTemplate template, string expectedSubjectPrefix)
    {
        var msg = Sut.Create(new NotificationInput(template, "en", "REQ-101", "Colombo"));

        Assert.Equal("en", msg.Locale);
        Assert.StartsWith(expectedSubjectPrefix, msg.Subject);
        Assert.Contains("REQ-101", msg.Body);
        Assert.Contains("Colombo", msg.Body);
    }

    [Theory]
    [InlineData("si", NotificationTemplate.RequestReceived)]
    [InlineData("si-LK", NotificationTemplate.AidDispatched)]
    [InlineData("SI", NotificationTemplate.RequestFulfilled)]
    public void Create_BuildsSinhalaMessage_ForSiLocale(string locale, NotificationTemplate template)
    {
        var msg = Sut.Create(new NotificationInput(template, locale, "REQ-202", "Galle"));

        Assert.Equal("si", msg.Locale);
        Assert.Contains("REQ-202", msg.Subject);
        Assert.Contains("Galle", msg.Body);
        Assert.Contains("ආධාර", msg.Subject);
    }

    [Theory]
    [InlineData("ta", NotificationTemplate.RequestReceived)]
    [InlineData("ta-LK", NotificationTemplate.AidDispatched)]
    [InlineData("TA", NotificationTemplate.RequestFulfilled)]
    public void Create_BuildsTamilMessage_ForTaLocale(string locale, NotificationTemplate template)
    {
        var msg = Sut.Create(new NotificationInput(template, locale, "REQ-303", "Jaffna"));

        Assert.Equal("ta", msg.Locale);
        Assert.Contains("REQ-303", msg.Subject);
        Assert.Contains("Jaffna", msg.Body);
        Assert.Contains("உதவி", msg.Subject);
    }

    [Fact]
    public void Create_FallsBackToEnglish_WhenLocaleUnknown()
    {
        var msg = Sut.Create(new NotificationInput(NotificationTemplate.RequestReceived, "fr", "REQ-404", "Kandy"));

        Assert.Equal("en", msg.Locale);
        Assert.Contains("REQ-404", msg.Body);
    }

    [Fact]
    public void Create_Throws_WhenReferenceCodeMissing()
    {
        Assert.Throws<ArgumentException>(() =>
            Sut.Create(new NotificationInput(NotificationTemplate.RequestReceived, "en", "", "Kandy")));
    }
}
