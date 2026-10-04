using Shared.Enums;
using Shared.Models.Authentication;

namespace Shared.Models.Organizations;

public sealed record AcceptInvitationResult(
    AcceptInvitationStatus Status,
    IReadOnlyDictionary<string, string[]>? Errors = null,
    AuthSession? Session = null);
