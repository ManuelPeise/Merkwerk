using Data.Accessor.Abstractions;
using Data.Database;
using Data.Database.Entities.Devices;
using Microsoft.EntityFrameworkCore;

namespace Data.Accessor.Repositories;

internal sealed class DeviceRepository : EntityRepository<DeviceEntity>, IDeviceRepository
{
    public DeviceRepository(MerkwerkDbContext context)
        : base(context)
    {
    }

    public Task<DeviceEntity?> FindByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default) =>
        // IgnoreQueryFilters: the device request carries no organization (AGENTS.md §5, documented exception).
        Set.IgnoreQueryFilters().FirstOrDefaultAsync(d => d.TokenHash == tokenHash, cancellationToken);
}
