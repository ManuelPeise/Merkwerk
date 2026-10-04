using Data.Database.Entities.Devices;

namespace Data.Accessor.Abstractions;

public interface IDeviceRepository : IRepository<DeviceEntity>
{
    /// <summary>
    /// Tracked device by token hash, across organizations: a device identifies itself only by its cookie, the request
    /// carries no organization (LP-106, documented exception to the tenant filter).
    /// </summary>
    Task<DeviceEntity?> FindByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);
}
