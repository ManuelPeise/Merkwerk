using Data.Accessor.Abstractions;
using Data.Database;
using Data.Database.Entities.Organizations;
using Microsoft.EntityFrameworkCore;

namespace Data.Accessor.Repositories;

internal sealed class OrganizationRepository(MerkwerkDbContext context)
    : EntityRepository<OrganizationEntity>(context), IOrganizationRepository
{
    public Task<bool> AnyAsync(CancellationToken cancellationToken = default) => Set.AnyAsync(cancellationToken);
}
