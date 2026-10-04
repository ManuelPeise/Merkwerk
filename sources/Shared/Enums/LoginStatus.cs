namespace Shared.Enums;

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

    /// <summary>
    /// Password correct, but the adult belongs to no family (any more), e.g. after being removed (LP-107). Without a
    /// membership there is no role, so no session is started.
    /// </summary>
    NoMembership,
}
