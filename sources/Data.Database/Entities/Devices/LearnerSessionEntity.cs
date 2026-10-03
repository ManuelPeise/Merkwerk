using Data.Database.Entities.Base;
using Data.Database.Entities.Learners;

namespace Data.Database.Entities.Devices;

/// <summary>
/// A child's session on a paired device (LP-106): ends 8 hours after the sign-in, when the child is switched, or when the
/// device is unpaired. Only the SHA-256 hash of its refresh token is stored.
/// </summary>
public sealed class LearnerSessionEntity : AOrganizationEntityBase
{
    public const int TokenHashLength = 64;

    public long DeviceId { get; set; }

    public long LearnerId { get; set; }

    /// <summary>Hex SHA-256 of the refresh token.</summary>
    public string TokenHash { get; set; } = string.Empty;

    /// <summary>UTC; fixed at sign-in (no sliding).</summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>UTC; null = still usable.</summary>
    public DateTime? RevokedAt { get; set; }

    public DeviceEntity? Device { get; set; }

    public LearnerEntity? Learner { get; set; }
}
