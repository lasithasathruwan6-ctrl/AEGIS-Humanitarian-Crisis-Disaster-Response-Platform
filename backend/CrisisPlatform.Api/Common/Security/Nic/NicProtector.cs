using System.Security.Cryptography;
using System.Text;

namespace CrisisPlatform.Api.Common.Security.Nic;

/// <summary>
/// Encrypts NIC numbers at rest with AES-256-GCM. Layout: version(1) | nonce(12) | tag(16) | ciphertext, Base64.
/// Any change to the stored value makes decryption fail instead of returning altered data.
/// </summary>
public sealed class NicProtector
{
    private const byte Version = 1;
    private const int NonceSize = 12;
    private const int TagSize = 16;
    private static readonly byte[] AssociatedData = Encoding.UTF8.GetBytes("crisis-platform:nic:v1");

    private readonly byte[] _key;

    public NicProtector(byte[] key)
    {
        if (key.Length != 32)
        {
            throw new ArgumentException("NIC encryption key must be exactly 32 bytes (AES-256).", nameof(key));
        }

        _key = (byte[])key.Clone();
    }

    public string Protect(string nic)
    {
        var validation = NicValidator.Validate(nic);
        if (!validation.IsValid)
        {
            throw new ArgumentException(validation.Error, nameof(nic));
        }

        var plain = Encoding.UTF8.GetBytes(validation.Normalised!);
        var nonce = RandomNumberGenerator.GetBytes(NonceSize);
        var cipher = new byte[plain.Length];
        var tag = new byte[TagSize];

        using var aes = new AesGcm(_key, TagSize);
        aes.Encrypt(nonce, plain, cipher, tag, AssociatedData);

        var blob = new byte[1 + NonceSize + TagSize + cipher.Length];
        blob[0] = Version;
        nonce.CopyTo(blob, 1);
        tag.CopyTo(blob, 1 + NonceSize);
        cipher.CopyTo(blob, 1 + NonceSize + TagSize);
        return Convert.ToBase64String(blob);
    }

    /// <exception cref="CryptographicException">The value is malformed, tampered with or was encrypted with another key.</exception>
    public string Unprotect(string protectedNic)
    {
        byte[] blob;
        try
        {
            blob = Convert.FromBase64String(protectedNic);
        }
        catch (FormatException)
        {
            throw new CryptographicException("Protected NIC is not valid Base64.");
        }

        if (blob.Length <= 1 + NonceSize + TagSize || blob[0] != Version)
        {
            throw new CryptographicException("Protected NIC has an unknown format.");
        }

        var nonce = blob.AsSpan(1, NonceSize);
        var tag = blob.AsSpan(1 + NonceSize, TagSize);
        var cipher = blob.AsSpan(1 + NonceSize + TagSize);
        var plain = new byte[cipher.Length];

        using var aes = new AesGcm(_key, TagSize);
        aes.Decrypt(nonce, cipher, tag, plain, AssociatedData);

        return Encoding.UTF8.GetString(plain);
    }
}
