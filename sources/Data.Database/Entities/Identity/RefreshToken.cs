using Data.Database.Entities.Base;

namespace Data.Database.Entities.Identity;

/// <summary>
/// One refresh token (ADR 013). Only the SHA-256 hash is stored. Every refresh revokes the token and issues a new one in
/// the same chain; redeeming a revoked token revokes the whole chain (token theft).
/// </summary>
public sealed class RefreshToken : AEntityBase
{
    public const int TokenHashLength = 64;

    public long UserId { get; set; }

    /// <summary>Hex SHA-256 of the token.</summary>
    public string TokenHash { get; set; } = string.Empty;

    /// <summary>All tokens of one login share the chain id.</summary>
    public Guid ChainId { get; set; }

    /// <summary>UTC.</summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>UTC; null = still usable.</summary>
    public DateTime? RevokedAt { get; set; }
}
