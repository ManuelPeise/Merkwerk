using Data.Database.Entities.Organizations;
using Logic.Authentication;

namespace Logic.Organizations.Invitations;

public enum InvitationState
{
    Pending,
    Expired,
}
