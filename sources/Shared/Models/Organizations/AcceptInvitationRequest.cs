namespace Shared.Models.Organizations;

/// <summary>New account: DisplayName, Password and PrivacyAccepted are needed. Signed in: only the token.</summary>
public sealed record AcceptInvitationRequest(string Token, string? DisplayName, string? Password, bool PrivacyAccepted);
