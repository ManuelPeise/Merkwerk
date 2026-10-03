namespace Shared.Enums;

public enum RemoveMemberStatus
{
    Success,

    /// <summary>404 – also when the acting user is no admin (nothing to reveal).</summary>
    NotFound,

    /// <summary>The owner cannot be removed (409).</summary>
    IsOwner,

    /// <summary>Admins cannot remove themselves (409) – the family would risk ending up without an admin.</summary>
    IsSelf,
}
