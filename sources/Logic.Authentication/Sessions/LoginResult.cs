namespace Logic.Authentication.Sessions;

public enum LoginStatus
{
    Success,

    /// <summary>
    /// Unknown e-mail, wrong password, expired start password or locked account (5 failed attempts, 15 minutes) –
    /// deliberately one answer, so nobody can find out which addresses are registered.
    /// </summary>
    InvalidCredentials,

    /// <summary>Password correct, but the e-mail address is not confirmed yet.</summary>
    EmailNotConfirmed,
}

/// <summary>Outcome of a login; <see cref="Session"/> is set only for <see cref="LoginStatus.Success"/>.</summary>
public sealed record LoginResult(LoginStatus Status, AuthSession? Session = null);
