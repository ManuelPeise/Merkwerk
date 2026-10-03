using Data.Database.Entities.Learners;

namespace Data.Accessor.Abstractions;

public interface ILearnerRepository : IRepository<LearnerEntity>
{
    /// <summary>
    /// Children of the given family for the profile selection of a paired device – the device request carries no
    /// organization claim (LP-106, documented exception; filtered explicitly by <paramref name="organizationId"/>).
    /// </summary>
    Task<IReadOnlyList<LearnerEntity>> ListForDeviceAsync(long organizationId, CancellationToken cancellationToken = default);

    /// <summary>The child, if it belongs to the given family (child sign-in on a paired device, LP-106).</summary>
    Task<LearnerEntity?> FindForDeviceAsync(long organizationId, long learnerId, CancellationToken cancellationToken = default);
}
