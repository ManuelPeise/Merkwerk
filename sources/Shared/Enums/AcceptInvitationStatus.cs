namespace Shared.Enums;

public enum AcceptInvitationStatus
{
    Success,

    /// <summary>404.</summary>
    NotFound,

    /// <summary>Expired, used or withdrawn (410).</summary>
    Gone,

    /// <summary>Not signed in, but an account with the invited address exists – sign in first (409).</summary>
    AccountExists,

    /// <summary>Signed in with a different address than the invited one (403).</summary>
    EmailMismatch,

    /// <summary>Missing fields or password rules (400, see Errors).</summary>
    Invalid,
}
