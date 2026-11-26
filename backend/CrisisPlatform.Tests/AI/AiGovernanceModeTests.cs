using CrisisPlatform.Api.Modules.AI;

namespace CrisisPlatform.Tests.AI;

public class AiGovernanceModeTests
{
    [Fact]
    public void SwitchMode_ActiveShadowOff_ShouldPersistAndRecordAuditEntries()
    {
        var gov = new AiGovernanceService();
        Assert.Equal(AiOperatingMode.Active, gov.CurrentMode);

        gov.SwitchMode(AiOperatingMode.Shadow, "admin-1", "Silent evaluation during flood surge");
        Assert.Equal(AiOperatingMode.Shadow, gov.CurrentMode);

        gov.SwitchMode(AiOperatingMode.Off, "admin-1", "Emergency kill switch activated");
        Assert.Equal(AiOperatingMode.Off, gov.CurrentMode);

        Assert.Equal(2, gov.AuditLog.Count);
        Assert.Equal(AiOperatingMode.Active, gov.AuditLog[0].PreviousMode);
        Assert.Equal(AiOperatingMode.Shadow, gov.AuditLog[0].NewMode);
        Assert.Equal(AiOperatingMode.Off, gov.AuditLog[1].NewMode);
    }
}
