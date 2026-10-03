namespace Shared.Models.Organizations;

public sealed record SetupRequest(string FamilyName, string DisplayName, string Email, string Password, bool PrivacyAccepted);
