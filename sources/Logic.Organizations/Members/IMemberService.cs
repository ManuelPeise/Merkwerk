namespace Logic.Organizations.Members;

/// <summary>Adults of a family and their roles (LP-105).</summary>
public interface IMemberService
{
    Task<IReadOnlyList<MemberInfo>> ListAsync(long organizationId, CancellationToken cancellationToken);

    Task<RemoveMemberStatus> RemoveAsync(
        long organizationId,
        long actingUserId,
        long membershipId,
        CancellationToken cancellationToken);

    Task<bool> IsMemberAsync(long organizationId, long userId, CancellationToken cancellationToken);

    Task<bool> IsAdminAsync(long organizationId, long userId, CancellationToken cancellationToken);
}
