namespace Shared.Enums;

public enum InvitationLookupStatus
{
    Found,

    /// <summary>Unknown token (404).</summary>
    NotFound,

    /// <summary>Expired, used or withdrawn (410).</summary>
    Gone,
}
