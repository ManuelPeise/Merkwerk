using Microsoft.AspNetCore.Identity;

namespace Data.Database.Entities.Identity;

/// <summary>
/// An adult account (ASP.NET Core Identity, LP-104). Children have no account – they sign in on paired devices (LP-106).
/// Exception to ADR 011: Identity dictates the base class, so there are no audit fields here.
/// The role belongs to the membership in an organization (LP-105/LP-107), not to the user.
/// </summary>
public sealed class User : IdentityUser<long>
{
    public const int DisplayNameMaxLength = 100;

    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Set after an admin reset (start password): every endpoint except change-password answers 403.</summary>
    public bool MustChangePassword { get; set; }

    /// <summary>UTC. The start password is only valid until then (24 hours).</summary>
    public DateTime? StartPasswordExpiresAt { get; set; }
}
