using Data.Database.Entities.Organizations;
using Logic.Authentication;

namespace Logic.Organizations.Invitations;

public sealed record AcceptInvitationResult(
    AcceptInvitationStatus Status,
    IReadOnlyDictionary<string, string[]>? Errors = null,
    AuthSession? Session = null);
