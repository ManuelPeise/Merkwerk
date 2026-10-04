using Data.Accessor.Abstractions;
using Data.Database;
using Data.Database.Entities.Devices;
using Microsoft.EntityFrameworkCore;

namespace Data.Accessor.Repositories;

internal sealed class PairingCodeRepository : EntityRepository<PairingCodeEntity>, IPairingCodeRepository
{
    public PairingCodeRepository(MerkwerkDbContext context)
        : base(context)
    {
    }

    public Task<PairingCodeEntity?> FindUsableByHashAsync(string codeHash, DateTime now, CancellationToken cancellationToken = default) =>
        // IgnoreQueryFilters: the device is not part of a family yet (AGENTS.md §5, documented exception).
        Set.IgnoreQueryFilters()
            .Where(c => c.CodeHash == codeHash && c.UsedAt == null && c.RevokedAt == null && c.ExpiresAt > now)
            .OrderByDescending(c => c.Id)
            .FirstOrDefaultAsync(cancellationToken);
}
