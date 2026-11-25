using CrisisPlatform.Api.Common.Security.Authorization;

namespace CrisisPlatform.Api.Modules.Admin.RoleChanges;

public enum RoleChangeState
{
    Pending,
    Approved,
}

public sealed class RoleChangeRequest
{
    public required Guid Id { get; init; }
    public required string TargetUserId { get; init; }
    public required string NewRole { get; init; }
    public required string RequestedBy { get; init; }
    public required DateTimeOffset RequestedAt { get; init; }
    public required DateTimeOffset ExpiresAt { get; init; }
    public RoleChangeState State { get; set; } = RoleChangeState.Pending;
    public string? ApprovedBy { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
}

public interface IRoleChangeStore
{
    Task AddAsync(RoleChangeRequest request, CancellationToken cancellationToken = default);

    Task<RoleChangeRequest?> FindAsync(Guid id, CancellationToken cancellationToken = default);

    Task SaveAsync(RoleChangeRequest request, CancellationToken cancellationToken = default);
}

public enum RoleChangeDecision
{
    Approved,
    NotFound,
    SameAsRequester,
    SameAsTarget,
    AlreadyApproved,
    Expired,
}

/// <summary>
/// Four-eyes control: a role change only takes effect when an Admin other than the requester approves it.
/// The caller applies the new role when <see cref="RoleChangeDecision.Approved"/> is returned.
/// </summary>
public sealed class RoleChangeService(IRoleChangeStore store, TimeProvider clock)
{
    public static readonly TimeSpan PendingLifetime = TimeSpan.FromHours(72);

    private static readonly string[] AssignableRoles =
        [Roles.Citizen, Roles.Volunteer, Roles.Staff, Roles.Coordinator, Roles.Admin];

    public async Task<Guid> RequestAsync(
        string targetUserId, string newRole, string requestedBy, CancellationToken cancellationToken = default)
    {
        if (!AssignableRoles.Contains(newRole))
        {
            throw new ArgumentException($"Unknown role '{newRole}'.", nameof(newRole));
        }

        var now = clock.GetUtcNow();
        var request = new RoleChangeRequest
        {
            Id = Guid.NewGuid(),
            TargetUserId = targetUserId,
            NewRole = newRole,
            RequestedBy = requestedBy,
            RequestedAt = now,
            ExpiresAt = now + PendingLifetime,
        };

        await store.AddAsync(request, cancellationToken);
        return request.Id;
    }

    public async Task<RoleChangeDecision> ApproveAsync(
        Guid requestId, string approverId, CancellationToken cancellationToken = default)
    {
        var request = await store.FindAsync(requestId, cancellationToken);
        if (request is null)
        {
            return RoleChangeDecision.NotFound;
        }

        if (request.State == RoleChangeState.Approved)
        {
            return RoleChangeDecision.AlreadyApproved;
        }

        if (string.Equals(request.RequestedBy, approverId, StringComparison.Ordinal))
        {
            return RoleChangeDecision.SameAsRequester;
        }

        if (string.Equals(request.TargetUserId, approverId, StringComparison.Ordinal))
        {
            return RoleChangeDecision.SameAsTarget;
        }

        var now = clock.GetUtcNow();
        if (now >= request.ExpiresAt)
        {
            return RoleChangeDecision.Expired;
        }

        request.State = RoleChangeState.Approved;
        request.ApprovedBy = approverId;
        request.ApprovedAt = now;
        await store.SaveAsync(request, cancellationToken);
        return RoleChangeDecision.Approved;
    }
}
