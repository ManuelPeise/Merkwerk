namespace Shared.Enums;

/// <summary>Available mail templates. Each one exists as HTML and text in every supported language.</summary>
public enum MailTemplate
{
    /// <summary>Values: Name, Link.</summary>
    ConfirmEmail,

    /// <summary>Values: Name, InvitedBy, OrganizationName, Link, ExpiresAt.</summary>
    Invitation,

    /// <summary>Values: Name, Link, ExpiresAt.</summary>
    PasswordReset,

    /// <summary>Start password / one-time code (LP-104). Values: Name, Code, ExpiresAt.</summary>
    OneTimeCode,
}
