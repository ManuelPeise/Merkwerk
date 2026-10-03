using Data.Database.Entities.Devices;

namespace Data.Accessor.Abstractions;

public interface ILearnerSessionRepository : IRepository<LearnerSessionEntity>
{
    /// <summary>
    /// Tracked session by refresh token hash with its device and learner, across organizations: the refresh request has
    /// no valid access token any more (LP-106, documented exception to the tenant filter).
    /// </summary>
    Task<LearnerSessionEntity?> FindByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);
}
