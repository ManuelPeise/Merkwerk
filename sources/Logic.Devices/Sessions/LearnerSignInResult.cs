namespace Logic.Devices.Sessions;

/// <summary>
/// Outcome of a child sign-in. On success <see cref="DeviceTokenExpiresAt"/> is the extended lifetime of the device
/// (the caller renews the device cookie with it).
/// </summary>
public sealed record LearnerSignInResult(
    LearnerSignInStatus Status,
    Logic.Authentication.AuthSession? Session = null,
    DateTimeOffset? DeviceTokenExpiresAt = null);
