using Data.Database.Entities.Organizations;
using Logic.Authentication;

namespace Logic.Organizations.Invitations;

/// <summary>An open invitation as the family's admins see it.</summary>
public sealed record InvitationInfo(long Id, string Email, OrganizationRole Role, DateTimeOffset ExpiresAt, InvitationState State);
