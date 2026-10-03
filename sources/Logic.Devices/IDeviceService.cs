using Logic.Devices.Pairing;

namespace Logic.Devices;

/// <summary>
/// Pairing devices with a family and managing them (LP-106). Adults create codes, list and unpair devices; a device
/// identifies itself only by its token (cookie) – the caller passes it in, transport stays in Web.Core.
/// </summary>
public interface IDeviceService
{
    /// <summary>
    /// New six-digit code (10 minutes, single use); replaces the family's open code. <c>null</c> if the acting user is no
    /// member of the organization (any more).
    /// </summary>
    Task<PairingCodeInfo?> CreatePairingCodeAsync(long organizationId, long actingUserId, CancellationToken cancellationToken);

    /// <summary>Pairs the calling device with the family of the code and returns its device token.</summary>
    Task<PairResult> PairAsync(string code, string deviceName, CancellationToken cancellationToken);

    /// <summary><c>null</c> if the token is unknown, unpaired or expired.</summary>
    Task<DeviceStatus?> GetStatusAsync(string deviceToken, CancellationToken cancellationToken);

    /// <summary>Children of the device's family; <c>null</c> if the device is not paired (any more).</summary>
    Task<IReadOnlyList<DeviceProfile>?> ListProfilesAsync(string deviceToken, CancellationToken cancellationToken);

    /// <summary>Paired devices of the family; <c>null</c> if the acting user is no member.</summary>
    Task<IReadOnlyList<DeviceInfo>?> ListAsync(long organizationId, long actingUserId, CancellationToken cancellationToken);

    /// <summary>Unpairs the device and ends its children's sessions. <c>false</c> if it does not exist in the family.</summary>
    Task<bool> RevokeAsync(long organizationId, long actingUserId, long deviceId, CancellationToken cancellationToken);
}
