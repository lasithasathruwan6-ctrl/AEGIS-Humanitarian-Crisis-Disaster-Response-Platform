using System.Security.Cryptography;
using System.Text;
using CrisisPlatform.Api.Common.Security.Authorization;
using Microsoft.AspNetCore.WebUtilities;

namespace CrisisPlatform.Api.Modules.Admin.Invitations;

public sealed class InvitationRecord
{
    /// <summary>SHA-256 of the invitation token; the token itself only travels in the invitation email.</summary>
    public required string TokenHash { get; init; }

    public required string Email { get; init; }
    public required string Role { get; init; }
    public required string InvitedBy { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public required DateTimeOffset ExpiresAt { get; init; }
    public DateTimeOffset? UsedAt { get; set; }
}

public interface IInvitationStore
{
    Task AddAsync(InvitationRecord record, CancellationToken cancellationToken = default);

    Task<InvitationRecord?> FindByHashAsync(string tokenHash, CancellationToken cancellationToken = default);

    /// <summary>Atomically marks the invitation used; returns false if it was already used.</summary>
    Task<bool> TryMarkUsedAsync(string tokenHash, DateTimeOffset usedAt, CancellationToken cancellationToken = default);
}

public enum InvitationStatus
{
    Accepted,
    Invalid,
    Expired,
    AlreadyUsed,
}

public sealed record InvitationResult(InvitationStatus Status, string? Email = null, string? Role = null);

public sealed class StaffInvitationService(IInvitationStore store, TimeProvider clock)
{
    public static readonly TimeSpan Lifetime = TimeSpan.FromHours(24);

    /// <summary>Roles that may be granted by invitation. Admin is deliberately excluded (see role change approval).</summary>
    private static readonly string[] InvitableRoles = [Roles.Volunteer, Roles.Staff, Roles.Coordinator];

    /// <returns>The plain token to place in the invitation link.</returns>
    public async Task<string> CreateAsync(string email, string role, string invitedBy, CancellationToken cancellationToken = default)
    {
        if (!InvitableRoles.Contains(role))
        {
            throw new ArgumentException($"Role '{role}' cannot be granted by invitation.", nameof(role));
        }

        var token = WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(32));
        var now = clock.GetUtcNow();

        await store.AddAsync(new InvitationRecord
        {
            TokenHash = Hash(token),
            Email = email.Trim().ToLowerInvariant(),
            Role = role,
            InvitedBy = invitedBy,
            CreatedAt = now,
            ExpiresAt = now + Lifetime,
        }, cancellationToken);

        return token;
    }

    public async Task<InvitationResult> AcceptAsync(string token, CancellationToken cancellationToken = default)
    {
        var hash = Hash(token ?? string.Empty);
        var record = await store.FindByHashAsync(hash, cancellationToken);

        if (record is null)
        {
            return new InvitationResult(InvitationStatus.Invalid);
        }

        if (record.UsedAt is not null)
        {
            return new InvitationResult(InvitationStatus.AlreadyUsed);
        }

        var now = clock.GetUtcNow();
        if (now >= record.ExpiresAt)
        {
            return new InvitationResult(InvitationStatus.Expired);
        }

        // The store call is the single-use guard, so two simultaneous requests cannot both succeed.
        return await store.TryMarkUsedAsync(hash, now, cancellationToken)
            ? new InvitationResult(InvitationStatus.Accepted, record.Email, record.Role)
            : new InvitationResult(InvitationStatus.AlreadyUsed);
    }

    private static string Hash(string token)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
