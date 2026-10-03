using Data.Database.Entities.Organizations;

namespace Data.Accessor.Abstractions;

public interface IMembershipRepository : IRepository<MembershipEntity>
{
    /// <summary>
    /// The membership a session is built from (the oldest one), across organizations. Used at login and refresh,
    /// when no organization is known yet – the only place besides invitations that bypasses the tenant filter.
    /// </summary>
    Task<MembershipEntity?> FindPrimaryForUserAsync(long userId, CancellationToken cancellationToken = default);

    /// <summary>Membership of the user in the given organization, across the tenant filter (accepting an invitation).</summary>
    Task<MembershipEntity?> FindAsync(long organizationId, long userId, CancellationToken cancellationToken = default);
}
