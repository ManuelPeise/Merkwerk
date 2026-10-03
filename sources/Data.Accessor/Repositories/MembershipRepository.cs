using Data.Accessor.Abstractions;
using Data.Database;
using Data.Database.Entities.Organizations;
using Microsoft.EntityFrameworkCore;

namespace Data.Accessor.Repositories;

internal sealed class MembershipRepository(MerkwerkDbContext context)
    : EntityRepository<MembershipEntity>(context), IMembershipRepository
{
    public Task<MembershipEntity?> FindPrimaryForUserAsync(long userId, CancellationToken cancellationToken = default) =>
        // IgnoreQueryFilters: the session does not know its organization yet (AGENTS.md §5, documented exception).
        Set.IgnoreQueryFilters()
            .AsNoTracking()
            .Where(m => m.UserId == userId)
            .OrderBy(m => m.Id)
            .FirstOrDefaultAsync(cancellationToken);

    public Task<MembershipEntity?> FindAsync(long organizationId, long userId, CancellationToken cancellationToken = default) =>
        Set.IgnoreQueryFilters()
            .FirstOrDefaultAsync(m => m.OrganizationId == organizationId && m.UserId == userId, cancellationToken);
}
