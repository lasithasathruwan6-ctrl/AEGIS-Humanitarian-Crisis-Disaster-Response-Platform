using CrisisPlatform.Api.Modules.AI;

namespace CrisisPlatform.Tests.AI;

public class DonationAnomalyTests
{
    [Fact]
    public void Evaluate_HighValueAmount_ShouldFlagForReview()
    {
        var res = DonationAnomalyDetector.Evaluate(amount: 1_250_000m, recentPledgeCountBySameDonor: 1);

        Assert.True(res.IsFlagged);
        Assert.Contains("High-value pledge", res.Reason);
    }

    [Fact]
    public void Evaluate_RapidRepeatDonor_ShouldFlagForReview()
    {
        var res = DonationAnomalyDetector.Evaluate(amount: 5_000m, recentPledgeCountBySameDonor: 3);

        Assert.True(res.IsFlagged);
        Assert.Contains("Rapid-repeat", res.Reason);
    }

    [Fact]
    public void Evaluate_NormalDonation_ShouldNotFlag()
    {
        var res = DonationAnomalyDetector.Evaluate(amount: 25_000m, recentPledgeCountBySameDonor: 1);

        Assert.False(res.IsFlagged);
        Assert.Null(res.Reason);
    }
}
