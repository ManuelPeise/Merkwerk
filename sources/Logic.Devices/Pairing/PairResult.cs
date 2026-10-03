namespace Logic.Devices.Pairing;

/// <summary>
/// Outcome of pairing. On success <see cref="DeviceToken"/> is the plain device token – only ever handed to the device
/// (HttpOnly cookie), never logged.
/// </summary>
public sealed record PairResult(
    PairStatus Status,
    string? DeviceToken = null,
    DateTimeOffset? DeviceTokenExpiresAt = null,
    string? FamilyName = null);
