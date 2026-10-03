using Data.Database.Entities.Organizations;

namespace Data.Accessor.Abstractions;

public interface IMembershipRepository : IRepository<Membership>
{
    /// <summary>
    /// The membership a session is built from (the oldest one), across organizations. Used at login and refresh,
    /// when no organization is known yet – the only place besides invitations that bypasses the tenant filter.
    /// </summary>
    Task<Membership?> FindPrimaryForUserAsync(long userId, CancellationToken cancellationToken = default);

    /// <summary>Membership of the user in the given organization, across the tenant filter (accepting an invitation).</summary>
    Task<Membership?> FindAsync(long organizationId, long userId, CancellationToken cancellationToken = default);
}
