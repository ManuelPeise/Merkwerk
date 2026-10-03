using Shared.Enums;

namespace Shared.Models.Organizations;

/// <summary>An open invitation as the family's admins see it.</summary>
public sealed record InvitationInfo(long Id, string Email, OrganizationRole Role, DateTimeOffset ExpiresAt, InvitationState State);
