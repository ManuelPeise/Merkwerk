using Data.Database.Entities.Base;

namespace Data.Database.Entities.Devices;

/// <summary>Six-digit code an adult creates to pair a device (LP-106). Single use, 10 minutes, stored hashed only.</summary>
public sealed class PairingCodeEntity : AOrganizationEntityBase
{
    public const int CodeHashLength = 64;

    /// <summary>Hex SHA-256 of the code.</summary>
    public string CodeHash { get; set; } = string.Empty;

    /// <summary>UTC.</summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>UTC; set when a device was paired with the code.</summary>
    public DateTime? UsedAt { get; set; }

    /// <summary>UTC; set when a newer code of the family replaced it.</summary>
    public DateTime? RevokedAt { get; set; }

    public long CreatedByUserId { get; set; }
}
