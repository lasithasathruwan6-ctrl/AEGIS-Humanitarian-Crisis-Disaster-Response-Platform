using CrisisPlatform.Api.Common.Auditing;

namespace CrisisPlatform.Tests.Database;

public sealed class AuditHashChainTests
{
    [Fact]
    public void Verify_returns_valid_for_unmodified_hash_chain()
    {
        var actor = Guid.NewGuid();
        var t0 = new DateTimeOffset(2026, 6, 1, 10, 0, 0, TimeSpan.Zero);

        var e1 = AuditHashChain.CreateNext("RequestCreated", actor, "AidRequest", Guid.NewGuid(), t0);
        var e2 = AuditHashChain.CreateNext("RecommendationApproved", actor, "AiRecommendation", Guid.NewGuid(), t0.AddMinutes(1), e1.EntryHash);
        var e3 = AuditHashChain.CreateNext("ShelterUpdated", actor, "Shelter", Guid.NewGuid(), t0.AddMinutes(2), e2.EntryHash);

        var result = AuditHashChain.Verify([e1, e2, e3]);

        Assert.True(result.IsValid);
        Assert.Null(result.TamperedIndex);
    }

    [Fact]
    public void Verify_detects_tampered_row_payload()
    {
        var actor = Guid.NewGuid();
        var t0 = new DateTimeOffset(2026, 6, 1, 10, 0, 0, TimeSpan.Zero);

        var e1 = AuditHashChain.CreateNext("RequestCreated", actor, "AidRequest", Guid.NewGuid(), t0);
        var e2 = AuditHashChain.CreateNext("RecommendationApproved", actor, "AiRecommendation", Guid.NewGuid(), t0.AddMinutes(1), e1.EntryHash);
        var e3 = AuditHashChain.CreateNext("ShelterUpdated", actor, "Shelter", Guid.NewGuid(), t0.AddMinutes(2), e2.EntryHash);

        // Tamper with middle row's Action without recomputing hash
        var tamperedE2 = e2 with { Action = "RecommendationRejected" };

        var result = AuditHashChain.Verify([e1, tamperedE2, e3]);

        Assert.False(result.IsValid);
        Assert.Equal(1, result.TamperedIndex);
        Assert.Equal("EntryHash mismatch.", result.Reason);
    }

    [Fact]
    public void Verify_detects_broken_prev_hash_link_when_row_deleted()
    {
        var actor = Guid.NewGuid();
        var t0 = new DateTimeOffset(2026, 6, 1, 10, 0, 0, TimeSpan.Zero);

        var e1 = AuditHashChain.CreateNext("RequestCreated", actor, "AidRequest", Guid.NewGuid(), t0);
        var e2 = AuditHashChain.CreateNext("RecommendationApproved", actor, "AiRecommendation", Guid.NewGuid(), t0.AddMinutes(1), e1.EntryHash);
        var e3 = AuditHashChain.CreateNext("ShelterUpdated", actor, "Shelter", Guid.NewGuid(), t0.AddMinutes(2), e2.EntryHash);

        // Simulate deletion of middle row e2
        var result = AuditHashChain.Verify([e1, e3]);

        Assert.False(result.IsValid);
        Assert.Equal(1, result.TamperedIndex);
        Assert.Equal("PrevHash mismatch.", result.Reason);
    }
}
