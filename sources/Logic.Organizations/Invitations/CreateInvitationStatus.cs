using Data.Database.Entities.Organizations;
using Logic.Authentication;

namespace Logic.Organizations.Invitations;

public enum CreateInvitationStatus
{
    Created,

    /// <summary>The acting user is not an admin of the organization (403).</summary>
    Forbidden,

    /// <summary>An adult with this address already belongs to the family (409).</summary>
    AlreadyMember,
}
