using Data.Accessor.Abstractions;
using Data.Database;
using Data.Database.Entities.Organizations;
using Microsoft.EntityFrameworkCore;

namespace Data.Accessor.Repositories;

internal sealed class InvitationRepository : EntityRepository<InvitationEntity>, IInvitationRepository
{
    public InvitationRepository(MerkwerkDbContext context)
        : base(context)
    {
    }

    public Task<InvitationEntity?> FindByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default) =>
        // IgnoreQueryFilters: the invited person is not part of the organization yet (AGENTS.md §5, documented exception).
        Set.IgnoreQueryFilters().FirstOrDefaultAsync(i => i.TokenHash == tokenHash, cancellationToken);
}
