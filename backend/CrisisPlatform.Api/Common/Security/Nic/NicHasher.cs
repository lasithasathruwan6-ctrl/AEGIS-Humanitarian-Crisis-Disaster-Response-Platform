using System.Security.Cryptography;
using System.Text;

namespace CrisisPlatform.Api.Common.Security.Nic;

/// <summary>
/// Produces a keyed, deterministic fingerprint of a NIC so duplicates can be found with a
/// unique index without storing or searching the plain number. Old and new NIC formats of the
/// same person map to the same fingerprint.
/// </summary>
public sealed class NicHasher
{
    private readonly byte[] _key;

    public NicHasher(byte[] key)
    {
        if (key.Length < 32)
        {
            throw new ArgumentException("NIC hash key must be at least 32 bytes.", nameof(key));
        }

        _key = (byte[])key.Clone();
    }

    public string Hash(string nic)
    {
        var validation = NicValidator.Validate(nic);
        if (!validation.IsValid)
        {
            throw new ArgumentException(validation.Error, nameof(nic));
        }

        var canonical = ToCanonical(validation.Normalised!, validation.Format);
        return Convert.ToHexString(HMACSHA256.HashData(_key, Encoding.UTF8.GetBytes($"nic:{canonical}")));
    }

    /// <summary>Old format YYDDDSSSC(V|X) equals new format 19YYDDD0SSSC.</summary>
    private static string ToCanonical(string nic, NicFormat format)
        => format == NicFormat.Old
            ? $"19{nic[..2]}{nic[2..5]}0{nic[5..8]}{nic[8]}"
            : nic;
}

/// <summary>Storage port: an atomic insert backed by a unique index on the NIC hash.</summary>
public interface INicIndex
{
    /// <returns>False when the hash is already present.</returns>
    Task<bool> TryAddAsync(string nicHash, CancellationToken cancellationToken = default);
}

public sealed class NicUniquenessService(INicIndex index, NicHasher hasher)
{
    /// <returns>True when the NIC was new; false when someone already registered it.</returns>
    public Task<bool> TryRegisterAsync(string nic, CancellationToken cancellationToken = default)
        => index.TryAddAsync(hasher.Hash(nic), cancellationToken);
}
