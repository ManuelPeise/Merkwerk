namespace Logic.Authentication.Sessions;

public enum LoginStatus
{
    Success,

    /// <summary>Unknown e-mail, wrong password or expired start password – deliberately one answer.</summary>
    InvalidCredentials,

    /// <summary>Too many failed attempts (5), locked for 15 minutes.</summary>
    LockedOut,

    /// <summary>Password correct, but the e-mail address is not confirmed yet.</summary>
    EmailNotConfirmed,
}

/// <summary>Outcome of a login; <see cref="Session"/> is set only for <see cref="LoginStatus.Success"/>.</summary>
public sealed record LoginResult(LoginStatus Status, AuthSession? Session = null);
