using Data.Database.Entities.Organizations;
using Logic.Authentication;

namespace Logic.Organizations.Invitations;

public sealed record InvitationDetailsResult(InvitationLookupStatus Status, InvitationDetails? Details = null);
