using Data.Database.Entities.Base;
using Shared.Enums;

namespace Data.Database.Entities.Organizations;

/// <summary>Invitation of an adult into an organization (LP-105). Only the SHA-256 hash of the token is stored.</summary>
public sealed class InvitationEntity : AOrganizationEntityBase
{
    public const int EmailMaxLength = 256;
    public const int TokenHashLength = 64;
    public const int InvitedByNameMaxLength = 100;

    public string Email { get; set; } = string.Empty;

    public OrganizationRole Role { get; set; }

    public string TokenHash { get; set; } = string.Empty;

    /// <summary>UTC.</summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>UTC; set when the invitation was used.</summary>
    public DateTime? AcceptedAt { get; set; }

    /// <summary>UTC; set when an admin withdrew it (or a newer invitation for the same address replaced it).</summary>
    public DateTime? RevokedAt { get; set; }

    public long InvitedByUserId { get; set; }

    /// <summary>Display name of the inviting adult at the time of the invitation (shown on the invitation page).</summary>
    public string InvitedByName { get; set; } = string.Empty;
}
