namespace Logic.Authentication.Accounts;

/// <summary>Data for a new adult account. The e-mail counts as confirmed when the person proved it (invitation link) or set up the instance.</summary>
public sealed record NewAccount(
    string Email,
    string DisplayName,
    string Password,
    bool EmailConfirmed,
    string PrivacyPolicyVersion);
