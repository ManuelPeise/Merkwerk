using Shared.Enums;

namespace Shared.Models.Organizations;

public sealed record InvitationDetailsResult(InvitationLookupStatus Status, InvitationDetails? Details = null);
