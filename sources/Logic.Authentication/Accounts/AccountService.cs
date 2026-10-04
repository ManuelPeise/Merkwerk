using System.Globalization;
using System.Security.Cryptography;
using Data.Database.Entities.Identity;
using Logic.Authentication.Tokens;
using Logic.Notifications;
using Logic.Shared.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Enums;
using Shared.Models.Authentication;
using Shared.Models.Notifications;

namespace Logic.Authentication.Accounts;

/// <summary>
/// Account operations with mails (LP-104). Logs never contain e-mail addresses or tokens – only user ids and outcomes.
/// </summary>
internal sealed partial class AccountService : IAccountService
{
    private const int StartPasswordLength = 12;

    /// <summary>No 0/O, 1/l/I – the start password is typed from a mail.</summary>
    private const string StartPasswordAlphabet = "ABCDEFGHJKMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789";

    private const string InvalidTokenError = "The link is invalid or has expired.";

    private readonly UserManager<UserEntity> _userManager;
    private readonly AuthSessionService _sessions;
    private readonly RefreshTokenStore _refreshTokens;
    private readonly IMailService _mailService;
    private readonly IPublicLinkBuilder _links;
    private readonly IMailDateFormatter _dates;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<AccountService> _logger;

    public AccountService(
        UserManager<UserEntity> userManager,
        AuthSessionService sessions,
        RefreshTokenStore refreshTokens,
        IMailService mailService,
        IPublicLinkBuilder links,
        IMailDateFormatter dates,
        TimeProvider timeProvider,
        ILogger<AccountService> logger)
    {
        _userManager = userManager;
        _sessions = sessions;
        _refreshTokens = refreshTokens;
        _mailService = mailService;
        _links = links;
        _dates = dates;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task RequestPasswordResetAsync(string email, string language, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user is null || !user.EmailConfirmed)
        {
            return;
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var link = _links.Build("/reset-password", ("email", user.Email!), ("token", TokenEncoding.EncodeForUrl(token)));
        var expiresAt = _timeProvider.GetUtcNow().Add(AccountTokenLifetimes.LinkToken);

        await TrySendAsync(user, MailTemplate.PasswordReset, language, new Dictionary<string, string>
        {
            ["Name"] = user.DisplayName,
            ["Link"] = link,
            ["ExpiresAt"] = _dates.Format(expiresAt, language),
        }, cancellationToken);
    }

    public async Task<AccountResult> ResetPasswordAsync(
        string email,
        string token,
        string newPassword,
        CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(email);
        var decoded = TokenEncoding.DecodeFromUrl(token);

        if (user is null || decoded is null)
        {
            return AccountResult.Failed(InvalidTokenError);
        }

        var result = await _userManager.ResetPasswordAsync(user, decoded, newPassword);

        // Identity checks the token before the password rules. A wrong token must answer exactly like an unknown
        // address, otherwise this anonymous endpoint tells which addresses are registered.
        if (!result.Succeeded)
        {
            return result.Errors.Any(e => e.Code == nameof(IdentityErrorDescriber.InvalidToken))
                ? AccountResult.Failed(InvalidTokenError)
                : ToFailure(result);
        }

        user.MustChangePassword = false;
        user.StartPasswordExpiresAt = null;
        await _userManager.UpdateAsync(user);
        await _userManager.SetLockoutEndDateAsync(user, null);
        await _userManager.ResetAccessFailedCountAsync(user);
        await _refreshTokens.RevokeAllAsync(user.Id, cancellationToken);

        LogPasswordReset(user.Id);
        return AccountResult.Success();
    }

    public async Task<bool> IsPasswordResetTokenValidAsync(string email, string token, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(email);
        var decoded = TokenEncoding.DecodeFromUrl(token);

        if (user is null || decoded is null)
        {
            return false;
        }

        // A used token fails here as well: the reset changes the security stamp the token was bound to.
        return await _userManager.VerifyUserTokenAsync(
            user,
            _userManager.Options.Tokens.PasswordResetTokenProvider,
            UserManager<UserEntity>.ResetPasswordTokenPurpose,
            decoded);
    }

    public async Task SendEmailConfirmationAsync(long userId, string language, CancellationToken cancellationToken)
    {
        var user = await FindAsync(userId) ?? throw new InvalidOperationException($"User {userId} does not exist.");
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var link = _links.Build(
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

        var result = await _userManager.ConfirmEmailAsync(user, decoded);
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

        var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);

        if (!result.Succeeded)
        {
            return ToFailure(result);
        }

        user.MustChangePassword = false;
        user.StartPasswordExpiresAt = null;
        await _userManager.UpdateAsync(user);
        await _refreshTokens.RevokeAllAsync(user.Id, cancellationToken);

        LogPasswordChanged(user.Id);
        return AccountResult.Success(await _sessions.IssueSessionAsync(user, Guid.NewGuid(), cancellationToken));
    }

    public async Task<AccountResult> IssueStartPasswordAsync(long userId, string language, CancellationToken cancellationToken)
    {
        var user = await FindAsync(userId);

        if (user is null)
        {
            return AccountResult.Failed("The account does not exist.");
        }

        var startPassword = RandomNumberGenerator.GetString(StartPasswordAlphabet, StartPasswordLength);
        var expiresAt = _timeProvider.GetUtcNow().Add(AccountTokenLifetimes.StartPassword);

        if (await _userManager.HasPasswordAsync(user))
        {
            var removed = await _userManager.RemovePasswordAsync(user);
            if (!removed.Succeeded)
            {
                return ToFailure(removed);
            }
        }

        var added = await _userManager.AddPasswordAsync(user, startPassword);
        if (!added.Succeeded)
        {
            return ToFailure(added);
        }

        user.MustChangePassword = true;
        user.StartPasswordExpiresAt = expiresAt.UtcDateTime;
        await _userManager.UpdateAsync(user);
        await _userManager.UpdateSecurityStampAsync(user);
        await _userManager.SetLockoutEndDateAsync(user, null);
        await _userManager.ResetAccessFailedCountAsync(user);
        await _refreshTokens.RevokeAllAsync(user.Id, cancellationToken);

        await TrySendAsync(user, MailTemplate.OneTimeCode, language, new Dictionary<string, string>
        {
            ["Name"] = user.DisplayName,
            ["Code"] = startPassword,
            ["ExpiresAt"] = _dates.Format(expiresAt, language),
        }, cancellationToken);

        LogStartPasswordIssued(user.Id);
        return AccountResult.Success();
    }

    public async Task<AccountResult> CreateAccountAsync(NewAccount account, CancellationToken cancellationToken)
    {
        var user = new UserEntity
        {
            UserName = account.Email,
            Email = account.Email,
            EmailConfirmed = account.EmailConfirmed,
            DisplayName = account.DisplayName,
            PrivacyPolicyVersion = account.PrivacyPolicyVersion,
            PrivacyAcceptedAt = _timeProvider.GetUtcNow().UtcDateTime,
        };

        var result = await _userManager.CreateAsync(user, account.Password);
        return result.Succeeded ? AccountResult.Created(user.Id) : ToFailure(result);
    }

    public async Task DeleteAccountAsync(long userId, CancellationToken cancellationToken)
    {
        if (await FindAsync(userId) is { } user)
        {
            await _userManager.DeleteAsync(user);
        }
    }

    public Task EndSessionsAsync(long userId, CancellationToken cancellationToken) =>
        _refreshTokens.RevokeAllAsync(userId, cancellationToken);

    public async Task<long?> FindUserIdByEmailAsync(string email, CancellationToken cancellationToken) =>
        (await _userManager.FindByEmailAsync(email))?.Id;

    public async Task<IReadOnlyList<AccountInfo>> GetAccountsAsync(
        IReadOnlyCollection<long> userIds,
        CancellationToken cancellationToken) =>
        await _userManager.Users
            .Where(u => userIds.Contains(u.Id))
            .Select(u => new AccountInfo(u.Id, u.DisplayName, u.Email ?? string.Empty))
            .ToListAsync(cancellationToken);

    private Task<UserEntity?> FindAsync(long userId) => _userManager.FindByIdAsync(userId.ToString(CultureInfo.InvariantCulture));

    /// <summary>A failed mail must not turn into an error the caller could use to probe for accounts.</summary>
    private async Task TrySendAsync(
        UserEntity user,
        MailTemplate template,
        string language,
        IReadOnlyDictionary<string, string> values,
        CancellationToken cancellationToken)
    {
        try
        {
            await _mailService.SendAsync(
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
