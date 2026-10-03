using Data.Accessor.Abstractions;
using Data.Database;
using Data.Database.Entities.Learners;
using Microsoft.EntityFrameworkCore;

namespace Data.Accessor.Repositories;

internal sealed class LearnerRepository(MerkwerkDbContext context)
    : EntityRepository<LearnerEntity>(context), ILearnerRepository
{
    public async Task<IReadOnlyList<LearnerEntity>> ListForDeviceAsync(long organizationId, CancellationToken cancellationToken = default) =>
        // IgnoreQueryFilters: the device request carries no organization claim; the family is filtered explicitly
        // (AGENTS.md §5, documented exception).
        await Set.IgnoreQueryFilters()
            .AsNoTracking()
            .Where(l => l.OrganizationId == organizationId)
            .OrderBy(l => l.DisplayName)
            .ToListAsync(cancellationToken);

    public Task<LearnerEntity?> FindForDeviceAsync(long organizationId, long learnerId, CancellationToken cancellationToken = default) =>
        Set.IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == learnerId && l.OrganizationId == organizationId, cancellationToken);
}
