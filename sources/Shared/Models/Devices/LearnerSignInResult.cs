using Shared.Enums;
using Shared.Models.Authentication;

namespace Shared.Models.Devices;

/// <summary>
/// Outcome of a child sign-in. On success <see cref="DeviceTokenExpiresAt"/> is the extended lifetime of the device
/// (the caller renews the device cookie with it).
/// </summary>
public sealed record LearnerSignInResult(
    LearnerSignInStatus Status,
    AuthSession? Session = null,
    DateTimeOffset? DeviceTokenExpiresAt = null);
