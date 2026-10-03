using Data.Database.Entities.Devices;

namespace Data.Accessor.Abstractions;

public interface IDeviceRepository : IRepository<Device>
{
    /// <summary>
    /// Tracked device by token hash, across organizations: a device identifies itself only by its cookie, the request
    /// carries no organization (LP-106, documented exception to the tenant filter).
    /// </summary>
    Task<Device?> FindByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);
}
