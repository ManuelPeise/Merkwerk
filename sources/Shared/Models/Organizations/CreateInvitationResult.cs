using Shared.Enums;

namespace Shared.Models.Organizations;

public sealed record CreateInvitationResult(CreateInvitationStatus Status, InvitationInfo? Invitation = null);
