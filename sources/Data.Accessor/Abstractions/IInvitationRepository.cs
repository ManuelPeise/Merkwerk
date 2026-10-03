using Data.Database.Entities.Organizations;

namespace Data.Accessor.Abstractions;

public interface IInvitationRepository : IRepository<InvitationEntity>
{
    /// <summary>
    /// Tracked invitation by token hash, across organizations: whoever opens the link is not signed in to the family yet.
    /// </summary>
    Task<InvitationEntity?> FindByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);
}
