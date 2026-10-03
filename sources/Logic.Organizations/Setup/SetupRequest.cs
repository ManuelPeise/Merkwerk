namespace Logic.Organizations.Setup;

public sealed record SetupRequest(string FamilyName, string DisplayName, string Email, string Password, bool PrivacyAccepted);
