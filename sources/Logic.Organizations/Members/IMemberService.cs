namespace Logic.Organizations.Members;

/// <summary>Adults of a family and their roles (LP-105).</summary>
public interface IMemberService
{
    /// <summary>Null if the acting user is no member of the organization (any more).</summary>
    Task<IReadOnlyList<MemberInfo>?> ListAsync(long organizationId, long actingUserId, CancellationToken cancellationToken);

    Task<RemoveMemberStatus> RemoveAsync(
        long organizationId,
        long actingUserId,
        long membershipId,
        CancellationToken cancellationToken);

    /// <summary>
    /// An admin gives an adult of the same family a start password by mail (LP-104). False if the acting user is no admin
    /// or the target is no member – the caller cannot tell which (nothing to reveal).
    /// </summary>
    Task<bool> IssueStartPasswordAsync(
        long organizationId,
        long actingUserId,
        long targetUserId,
        string language,
        CancellationToken cancellationToken);

    Task<bool> IsMemberAsync(long organizationId, long userId, CancellationToken cancellationToken);

    Task<bool> IsAdminAsync(long organizationId, long userId, CancellationToken cancellationToken);
}
