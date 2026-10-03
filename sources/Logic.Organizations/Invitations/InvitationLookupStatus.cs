using Data.Database.Entities.Organizations;
using Logic.Authentication;

namespace Logic.Organizations.Invitations;

public enum InvitationLookupStatus
{
    Found,

    /// <summary>Unknown token (404).</summary>
    NotFound,

    /// <summary>Expired, used or withdrawn (410).</summary>
    Gone,
}
