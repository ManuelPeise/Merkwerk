using Data.Database.Entities.Organizations;
using Logic.Authentication;

namespace Logic.Organizations.Invitations;

public sealed record CreateInvitationResult(CreateInvitationStatus Status, InvitationInfo? Invitation = null);
