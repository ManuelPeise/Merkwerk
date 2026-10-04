using Data.Accessor.Abstractions;
using Data.Database;
using Data.Database.Entities.Devices;
using Microsoft.EntityFrameworkCore;

namespace Data.Accessor.Repositories;

internal sealed class LearnerSessionRepository : EntityRepository<LearnerSessionEntity>, ILearnerSessionRepository
{
    public LearnerSessionRepository(MerkwerkDbContext context)
        : base(context)
    {
    }

    public Task<LearnerSessionEntity?> FindByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default) =>
        // IgnoreQueryFilters: a refresh has no valid access token, hence no organization (AGENTS.md §5, documented exception).
        Set.IgnoreQueryFilters()
            .Include(s => s.Device)
            .Include(s => s.Learner)
            .FirstOrDefaultAsync(s => s.TokenHash == tokenHash, cancellationToken);
}
