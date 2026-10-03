using System.Globalization;
using System.Security.Cryptography;
using Data.Database.Entities.Identity;
using Logic.Authentication.Tokens;
using Logic.Notifications;
using Logic.Notifications.Formatting;
using Logic.Notifications.Links;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Logic.Authentication.Accounts;

/// <summary>
/// Account operations with mails (LP-104). Logs never contain e-mail addresses or tokens – only user ids and outcomes.
/// </summary>
internal sealed partial class AccountService(
    UserManager<User> userManager,
    AuthSessionService sessions,
    RefreshTokenStore refreshTokens,
    IMailService mailService,
    IPublicLinkBuilder links,
    IMailDateFormatter dates,
    TimeProvider timeProvider,
    ILogger<AccountService> logger) : IAccountService
{
    private const int StartPasswordLength = 12;

    /// <summary>No 0/O, 1/l/I – the start password is typed from a mail.</summary>
    private const string StartPasswordAlphabet = "ABCDEFGHJKMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789";

    private const string InvalidTokenError = "The link is invalid or has expired.";

    public async Task RequestPasswordResetAsync(string email, string language, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(email);

        if (user is null || !user.EmailConfirmed)
        {
            return;
        }

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var link = links.Build("/reset-password", ("email", user.Email!), ("token", TokenEncoding.EncodeForUrl(token)));
        var expiresAt = timeProvider.GetUtcNow().Add(AccountTokenLifetimes.LinkToken);

        await TrySendAsync(user, MailTemplate.PasswordReset, language, new Dictionary<string, string>
        {
            ["Name"] = user.DisplayName,
            ["Link"] = link,
            ["ExpiresAt"] = dates.Format(expiresAt, language),
        }, cancellationToken);
    }

    public async Task<AccountResult> ResetPasswordAsync(
        string email,
        string token,
        string newPassword,
        CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(email);
        var decoded = TokenEncoding.DecodeFromUrl(token);

        if (user is null || decoded is null)
        {
            return AccountResult.Failed(InvalidTokenError);
        }

        var result = await userManager.ResetPasswordAsync(user, decoded, newPassword);

        if (!result.Succeeded)
        {
            return ToFailure(result);
        }

        user.MustChangePassword = false;
        user.StartPasswordExpiresAt = null;
        await userManager.UpdateAsync(user);
        await userManager.SetLockoutEndDateAsync(user, null);
        await userManager.ResetAccessFailedCountAsync(user);
        await refreshTokens.RevokeAllAsync(user.Id, cancellationToken);

        LogPasswordReset(user.Id);
        return AccountResult.Success();
    }

    public async Task SendEmailConfirmationAsync(long userId, string language, CancellationToken cancellationToken)
    {
        var user = await FindAsync(userId) ?? throw new InvalidOperationException($"User {userId} does not exist.");
        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
        var link = links.Build(
            "/confirm-email",
            ("userId", user.Id.ToString(CultureInfo.InvariantCulture)),
            ("token", TokenEncoding.EncodeForUrl(token)));

        await TrySendAsync(user, MailTemplate.ConfirmEmail, language, new Dictionary<string, string>
        {
            ["Name"] = user.DisplayName,
            ["Link"] = link,
        }, cancellationToken);
    }

    public async Task<AccountResult> ConfirmEmailAsync(string userId, string token, CancellationToken cancellationToken)
    {
        var decoded = TokenEncoding.DecodeFromUrl(token);
        var user = long.TryParse(userId, NumberStyles.None, CultureInfo.InvariantCulture, out var id)
            ? await FindAsync(id)
            : null;

        if (user is null || decoded is null)
        {
            return AccountResult.Failed(InvalidTokenError);
        }

        var result = await userManager.ConfirmEmailAsync(user, decoded);
        return result.Succeeded ? AccountResult.Success() : AccountResult.Failed(InvalidTokenError);
    }

    public async Task<AccountResult> ChangePasswordAsync(
        long userId,
        string currentPassword,
        string newPassword,
        CancellationToken cancellationToken)
    {
        var user = await FindAsync(userId);

        if (user is null)
        {
            return AccountResult.Failed("The account does not exist.");
        }

        var result = await userManager.ChangePasswordAsync(user, currentPassword, newPassword);

        if (!result.Succeeded)
        {
            return ToFailure(result);
        }

        user.MustChangePassword = false;
        user.StartPasswordExpiresAt = null;
        await userManager.UpdateAsync(user);
        await refreshTokens.RevokeAllAsync(user.Id, cancellationToken);

        LogPasswordChanged(user.Id);
        return AccountResult.Success(await sessions.IssueSessionAsync(user, Guid.NewGuid(), cancellationToken));
    }

    public async Task<AccountResult> IssueStartPasswordAsync(long userId, string language, CancellationToken cancellationToken)
    {
        var user = await FindAsync(userId);

        if (user is null)
        {
            return AccountResult.Failed("The account does not exist.");
        }

        var startPassword = RandomNumberGenerator.GetString(StartPasswordAlphabet, StartPasswordLength);
        var expiresAt = timeProvider.GetUtcNow().Add(AccountTokenLifetimes.StartPassword);

        if (await userManager.HasPasswordAsync(user))
        {
            var removed = await userManager.RemovePasswordAsync(user);
            if (!removed.Succeeded)
            {
                return ToFailure(removed);
            }
        }

        var added = await userManager.AddPasswordAsync(user, startPassword);
        if (!added.Succeeded)
        {
            return ToFailure(added);
        }

        user.MustChangePassword = true;
        user.StartPasswordExpiresAt = expiresAt.UtcDateTime;
        await userManager.UpdateAsync(user);
        await userManager.UpdateSecurityStampAsync(user);
        await userManager.SetLockoutEndDateAsync(user, null);
        await userManager.ResetAccessFailedCountAsync(user);
        await refreshTokens.RevokeAllAsync(user.Id, cancellationToken);

        await TrySendAsync(user, MailTemplate.OneTimeCode, language, new Dictionary<string, string>
        {
            ["Name"] = user.DisplayName,
            ["Code"] = startPassword,
            ["ExpiresAt"] = dates.Format(expiresAt, language),
        }, cancellationToken);

        LogStartPasswordIssued(user.Id);
        return AccountResult.Success();
    }

    private Task<User?> FindAsync(long userId) => userManager.FindByIdAsync(userId.ToString(CultureInfo.InvariantCulture));

    /// <summary>A failed mail must not turn into an error the caller could use to probe for accounts.</summary>
    private async Task TrySendAsync(
        User user,
        MailTemplate template,
        string language,
        IReadOnlyDictionary<string, string> values,
        CancellationToken cancellationToken)
    {
        try
        {
            await mailService.SendAsync(
                new MailMessageRequest(user.Email!, user.DisplayName, template, language, values),
                cancellationToken);
        }
        catch (MailDeliveryException)
        {
            LogMailFailed(template, user.Id);
        }
    }

    private static AccountResult ToFailure(IdentityResult result) =>
        AccountResult.Failed(result.Errors.Select(e => e.Description).ToArray());

    [LoggerMessage(Level = LogLevel.Information, Message = "Password of user {UserId} was reset.")]
    private partial void LogPasswordReset(long userId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Password of user {UserId} was changed.")]
    private partial void LogPasswordChanged(long userId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Start password issued for user {UserId}.")]
    private partial void LogStartPasswordIssued(long userId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Mail {Template} for user {UserId} could not be delivered.")]
    private partial void LogMailFailed(MailTemplate template, long userId);
}
