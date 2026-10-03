using Data.Database.Entities.Organizations;
using Logic.Authentication;

namespace Logic.Organizations.Invitations;

/// <summary>What the invited person sees before accepting (anonymous).</summary>
public sealed record InvitationDetails(string FamilyName, string Email, string InvitedBy, DateTimeOffset ExpiresAt);
