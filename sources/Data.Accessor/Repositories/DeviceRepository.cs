using Data.Accessor.Abstractions;
using Data.Database;
using Data.Database.Entities.Devices;
using Microsoft.EntityFrameworkCore;

namespace Data.Accessor.Repositories;

internal sealed class DeviceRepository(MerkwerkDbContext context)
    : EntityRepository<Device>(context), IDeviceRepository
{
    public Task<Device?> FindByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default) =>
        // IgnoreQueryFilters: the device request carries no organization (AGENTS.md §5, documented exception).
        Set.IgnoreQueryFilters().FirstOrDefaultAsync(d => d.TokenHash == tokenHash, cancellationToken);
}
