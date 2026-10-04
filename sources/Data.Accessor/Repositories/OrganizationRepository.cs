using Data.Accessor.Abstractions;
using Data.Database;
using Data.Database.Entities.Organizations;
using Microsoft.EntityFrameworkCore;

namespace Data.Accessor.Repositories;

internal sealed class OrganizationRepository : EntityRepository<OrganizationEntity>, IOrganizationRepository
{
    public OrganizationRepository(MerkwerkDbContext context)
        : base(context)
    {
    }

    public Task<bool> AnyAsync(CancellationToken cancellationToken = default) => Set.AnyAsync(cancellationToken);
}
