namespace Shared.Models.Organizations;

/// <summary>What the invited person sees before accepting (anonymous).</summary>
public sealed record InvitationDetails(string FamilyName, string Email, string InvitedBy, DateTimeOffset ExpiresAt);
