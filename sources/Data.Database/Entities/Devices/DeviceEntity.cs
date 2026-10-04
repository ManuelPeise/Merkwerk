using Data.Database.Entities.Base;

namespace Data.Database.Entities.Devices;

/// <summary>
/// A tablet or phone paired with a family (LP-106, ADR 006). Children sign in on it by tapping their picture.
/// Only the SHA-256 hash of the device token is stored; the token itself lives in the HttpOnly cookie <c>mw_device</c>.
/// </summary>
public sealed class DeviceEntity : AOrganizationEntityBase
{
    public const int NameMaxLength = 50;
    public const int TokenHashLength = 64;

    public string Name { get; set; } = string.Empty;

    /// <summary>Hex SHA-256 of the device token.</summary>
    public string TokenHash { get; set; } = string.Empty;

    /// <summary>UTC. Extended on every child sign-in (sliding, 180 days).</summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>UTC of the last child sign-in; null = not used yet.</summary>
    public DateTime? LastSeenAt { get; set; }

    /// <summary>UTC; set when an adult unpaired the device.</summary>
    public DateTime? RevokedAt { get; set; }

    public long PairedByUserId { get; set; }
}
