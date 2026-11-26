using CrisisPlatform.Api.Modules.AI;

namespace CrisisPlatform.Tests.AI;

public class AssistantEmergencyContactsTests
{
    [Fact]
    public async Task AskAssistant_ShouldAppendConfiguredEmergencyContacts()
    {
        var contacts = new EmergencyContactsConfig(Dmc: "117", Police: "119", Ambulance: "1990");
        var client = new GroqAssistantClient(new HttpClient(), contacts: contacts);

        var res = await client.AskAssistantAsync("What should I do during a flood?");

        Assert.True(res.IsValid);
        Assert.Contains("DMC 117", res.Reply);
        Assert.Contains("Police 119", res.Reply);
        Assert.Contains("Ambulance 1990", res.Reply);
    }
}
