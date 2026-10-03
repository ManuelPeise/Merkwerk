using Logic.Authentication.Accounts;

namespace Logic.Authentication;

/// <summary>Password reset, e-mail confirmation, password change and start passwords (LP-104).</summary>
public interface IAccountService
{
    /// <summary>Sends a reset link if a confirmed account exists. Never reveals whether it does (callers always answer 204).</summary>
    Task RequestPasswordResetAsync(string email, string language, CancellationToken cancellationToken);

    /// <summary>Sets a new password with the token from the mail and revokes all sessions of the user.</summary>
    Task<AccountResult> ResetPasswordAsync(string email, string token, string newPassword, CancellationToken cancellationToken);

    /// <summary>Sends a confirmation link to the user's address (used when an address must be confirmed, e.g. after a change).</summary>
    Task SendEmailConfirmationAsync(long userId, string language, CancellationToken cancellationToken);

    Task<AccountResult> ConfirmEmailAsync(string userId, string token, CancellationToken cancellationToken);

    /// <summary>Changes the password, clears the start-password flag, revokes all other sessions and returns a new session.</summary>
    Task<AccountResult> ChangePasswordAsync(long userId, string currentPassword, string newPassword, CancellationToken cancellationToken);

    /// <summary>
    /// Admin reset (endpoint with LP-105): sets a random start password (24 h), forces a change at the next login,
    /// revokes all sessions and mails the password.
    /// </summary>
    Task<AccountResult> IssueStartPasswordAsync(long userId, string language, CancellationToken cancellationToken);
}
