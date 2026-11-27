using System.Security.Cryptography;
using System.Text;

namespace CrisisPlatform.Api.Common.Auditing;

public sealed record AuditEntry(
    Guid Id,
    string Action,
    Guid PerformedBy,
    string TargetEntity,
    Guid TargetId,
    DateTimeOffset Timestamp,
    string PrevHash,
    string EntryHash);

public sealed record AuditChainVerificationResult(
    bool IsValid,
    int? TamperedIndex = null,
    string? Reason = null);

/// <summary>
/// Builds and verifies a SHA-256 hash chain over <see cref="AuditEntry"/> rows
/// so any post-hoc row modification or deletion breaks the chain.
/// </summary>
public static class AuditHashChain
{
    public static readonly string GenesisHash = new('0', 64);

    public static string ComputeEntryHash(
        Guid id,
        string action,
        Guid performedBy,
        string targetEntity,
        Guid targetId,
        DateTimeOffset timestamp,
        string prevHash)
    {
        var canonical = $"{id:D}|{action}|{performedBy:D}|{targetEntity}|{targetId:D}|{timestamp.UtcDateTime:O}|{prevHash}";
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(canonical));
        return Convert.ToHexString(bytes);
    }

    public static AuditEntry CreateNext(
        string action,
        Guid performedBy,
        string targetEntity,
        Guid targetId,
        DateTimeOffset timestamp,
        string? prevHash = null,
        Guid? id = null)
    {
        var entryId = id ?? Guid.NewGuid();
        var previous = string.IsNullOrEmpty(prevHash) ? GenesisHash : prevHash;
        var hash = ComputeEntryHash(entryId, action, performedBy, targetEntity, targetId, timestamp, previous);
        return new AuditEntry(entryId, action, performedBy, targetEntity, targetId, timestamp, previous, hash);
    }

    public static AuditChainVerificationResult Verify(IReadOnlyList<AuditEntry> entries)
    {
        string expectedPrev = GenesisHash;

        for (int i = 0; i < entries.Count; i++)
        {
            var entry = entries[i];

            if (!string.Equals(entry.PrevHash, expectedPrev, StringComparison.OrdinalIgnoreCase))
            {
                return new AuditChainVerificationResult(false, i, "PrevHash mismatch.");
            }

            var recomputed = ComputeEntryHash(
                entry.Id,
                entry.Action,
                entry.PerformedBy,
                entry.TargetEntity,
                entry.TargetId,
                entry.Timestamp,
                entry.PrevHash);

            if (!string.Equals(entry.EntryHash, recomputed, StringComparison.OrdinalIgnoreCase))
            {
                return new AuditChainVerificationResult(false, i, "EntryHash mismatch.");
            }

            expectedPrev = entry.EntryHash;
        }

        return new AuditChainVerificationResult(true);
    }
}
