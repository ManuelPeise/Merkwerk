namespace Logic.Devices;

/// <summary>Fixed lifetimes of LP-106.</summary>
public static class DeviceLifetimes
{
    /// <summary>A pairing code is valid for 10 minutes.</summary>
    public static readonly TimeSpan PairingCode = TimeSpan.FromMinutes(10);

    /// <summary>A paired device stays paired for 180 days after its last child sign-in.</summary>
    public static readonly TimeSpan DeviceToken = TimeSpan.FromDays(180);

    /// <summary>A child's session (including refresh) ends 8 hours after the sign-in.</summary>
    public static readonly TimeSpan LearnerSession = TimeSpan.FromHours(8);
}
