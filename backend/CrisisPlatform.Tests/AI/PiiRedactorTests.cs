using CrisisPlatform.Api.Modules.AI;

namespace CrisisPlatform.Tests.AI;

public class PiiRedactorTests
{
    [Fact]
    public void Redact_ShouldRemoveOldAndNewNicFormats()
    {
        var input = "Citizen NIC 912345678V and spouse 199512345678 need shelter.";
        var redacted = PiiRedactor.Redact(input);

        Assert.DoesNotContain("912345678V", redacted);
        Assert.DoesNotContain("199512345678", redacted);
        Assert.Contains("[REDACTED_NIC]", redacted);
    }

    [Fact]
    public void Redact_ShouldRemovePhoneAndEmail()
    {
        var input = "Call 0771234567 or +94712345678, email citizen.help@example.lk.";
        var redacted = PiiRedactor.Redact(input);

        Assert.DoesNotContain("0771234567", redacted);
        Assert.DoesNotContain("+94712345678", redacted);
        Assert.DoesNotContain("citizen.help@example.lk", redacted);
        Assert.Contains("[REDACTED_PHONE]", redacted);
        Assert.Contains("[REDACTED_EMAIL]", redacted);
    }
}
