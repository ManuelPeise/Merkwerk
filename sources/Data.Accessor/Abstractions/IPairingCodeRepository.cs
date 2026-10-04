using Data.Database.Entities.Devices;

namespace Data.Accessor.Abstractions;

public interface IPairingCodeRepository : IRepository<PairingCodeEntity>
{
    /// <summary>
    /// Tracked code that is neither used, revoked nor expired, across organizations: the device entering it does not belong
    /// to a family yet (LP-106, documented exception to the tenant filter).
    /// </summary>
    Task<PairingCodeEntity?> FindUsableByHashAsync(string codeHash, DateTime now, CancellationToken cancellationToken = default);
}
