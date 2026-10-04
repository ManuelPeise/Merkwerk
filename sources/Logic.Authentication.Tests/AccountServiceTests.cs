using Logic.Authentication.Tests.Infrastructure;
using Shared.Enums;
using Shared.Models.Authentication;

namespace Logic.Authentication.Tests;

/// <summary>LP-104: reset, confirmation, password change and start password, following the links in the mails.</summary>
[Collection(AuthDatabaseCollection.Name)]
public sealed class AccountServiceTests(AuthDatabaseFixture database)
{
    private const string Password = "correct-horse-battery";
    private const string NewPassword = "a-brand-new-password";

    private readonly AuthTestContext _context = AuthTestContext.Create(database);

    [Fact]
    public async Task RequestPasswordResetAsync_UnknownEmail_SendsNoMail()
    {
        await _context.AccountsAsync(a => a.RequestPasswordResetAsync("nobody@example.org", "de", default));

        Assert.Empty(_context.Mail.Sent);
    }

    [Fact]
    public async Task ResetPasswordAsync_TokenFromMail_NewPasswordWorksOldDoesNotAndSessionsEnd()
    {
        // Arrange
        var user = await _context.CreateUserAsync(Password);
        var login = await _context.SessionsAsync(s => s.LoginAsync(user.Email!, Password, default));
        await _context.AccountsAsync(a => a.RequestPasswordResetAsync(user.Email!, "en", default));
        var token = _context.Mail.LinkValue(MailTemplate.PasswordReset, "token");

        // Act
        var reset = await _context.AccountsAsync(a => a.ResetPasswordAsync(user.Email!, token, NewPassword, default));

        // Assert
        Assert.True(reset.Succeeded);
        Assert.Equal("en", _context.Mail.Last(MailTemplate.PasswordReset).Language);
        Assert.Equal(LoginStatus.Success, (await LoginAsync(user.Email!, NewPassword)).Status);
        Assert.Equal(LoginStatus.InvalidCredentials, (await LoginAsync(user.Email!, Password)).Status);
        Assert.Null(await _context.SessionsAsync(s => s.RefreshAsync(login.Session!.RefreshToken, default)));
    }

    [Fact]
    public async Task ResetPasswordAsync_InvalidToken_Fails()
    {
        var user = await _context.CreateUserAsync(Password);

        var reset = await _context.AccountsAsync(a => a.ResetPasswordAsync(user.Email!, "not-a-token", NewPassword, default));

        Assert.False(reset.Succeeded);
        Assert.Equal(LoginStatus.Success, (await LoginAsync(user.Email!, Password)).Status);
    }

    [Fact]
    public async Task ResetPasswordAsync_WrongTokenForKnownEmail_AnswersLikeUnknownEmail()
    {
        // Arrange: a well-formed token of another account, so Identity really verifies it.
        var user = await _context.CreateUserAsync(Password);
        var other = await _context.CreateUserAsync(Password);
        await _context.AccountsAsync(a => a.RequestPasswordResetAsync(other.Email!, "de", default));
        var foreignToken = _context.Mail.LinkValue(MailTemplate.PasswordReset, "token");

        // Act
        var known = await _context.AccountsAsync(a => a.ResetPasswordAsync(user.Email!, foreignToken, NewPassword, default));
        var unknown = await _context.AccountsAsync(
            a => a.ResetPasswordAsync("nobody@example.org", foreignToken, NewPassword, default));

        // Assert
        Assert.False(known.Succeeded);
        Assert.Equal(unknown.Errors, known.Errors);
    }

    [Fact]
    public async Task ResetPasswordAsync_PasswordTooShort_FailsWithMessage()
    {
        var user = await _context.CreateUserAsync(Password);
        await _context.AccountsAsync(a => a.RequestPasswordResetAsync(user.Email!, "de", default));
        var token = _context.Mail.LinkValue(MailTemplate.PasswordReset, "token");

        var reset = await _context.AccountsAsync(a => a.ResetPasswordAsync(user.Email!, token, "short", default));

        Assert.False(reset.Succeeded);
        Assert.NotEmpty(reset.Errors);
    }

    [Fact]
    public async Task IsPasswordResetTokenValidAsync_TokenFromMail_IsValid()
    {
        var user = await _context.CreateUserAsync(Password);
        await _context.AccountsAsync(a => a.RequestPasswordResetAsync(user.Email!, "de", default));
        var token = _context.Mail.LinkValue(MailTemplate.PasswordReset, "token");

        var valid = await _context.AccountsAsync(a => a.IsPasswordResetTokenValidAsync(user.Email!, token, default));

        Assert.True(valid);
    }

    [Fact]
    public async Task IsPasswordResetTokenValidAsync_TokenAlreadyUsed_IsInvalid()
    {
        // Arrange
        var user = await _context.CreateUserAsync(Password);
        await _context.AccountsAsync(a => a.RequestPasswordResetAsync(user.Email!, "de", default));
        var token = _context.Mail.LinkValue(MailTemplate.PasswordReset, "token");
        await _context.AccountsAsync(a => a.ResetPasswordAsync(user.Email!, token, NewPassword, default));

        // Act
        var valid = await _context.AccountsAsync(a => a.IsPasswordResetTokenValidAsync(user.Email!, token, default));

        // Assert
        Assert.False(valid);
    }

    [Fact]
    public async Task IsPasswordResetTokenValidAsync_UnknownEmailOrGarbageToken_IsInvalid()
    {
        // Arrange: a real token, so only the address decides.
        var user = await _context.CreateUserAsync(Password);
        await _context.AccountsAsync(a => a.RequestPasswordResetAsync(user.Email!, "de", default));
        var token = _context.Mail.LinkValue(MailTemplate.PasswordReset, "token");

        // Act
        var unknownEmail = await _context.AccountsAsync(
            a => a.IsPasswordResetTokenValidAsync("nobody@example.org", token, default));
        var garbageToken = await _context.AccountsAsync(
            a => a.IsPasswordResetTokenValidAsync(user.Email!, "not-a-token", default));

        // Assert
        Assert.False(unknownEmail);
        Assert.False(garbageToken);
    }

    [Fact]
    public async Task ConfirmEmailAsync_TokenFromMail_ConfirmsAddress()
    {
        // Arrange
        var user = await _context.CreateUserAsync(Password, emailConfirmed: false);
        await _context.AccountsAsync(a => a.SendEmailConfirmationAsync(user.Id, "de", default));
        var userId = _context.Mail.LinkValue(MailTemplate.ConfirmEmail, "userId");
        var token = _context.Mail.LinkValue(MailTemplate.ConfirmEmail, "token");

        // Act
        var confirmed = await _context.AccountsAsync(a => a.ConfirmEmailAsync(userId, token, default));

        // Assert
        Assert.True(confirmed.Succeeded);
        Assert.Equal(LoginStatus.Success, (await LoginAsync(user.Email!, Password)).Status);
    }

    [Fact]
    public async Task IssueStartPasswordAsync_LoginRequiresChange_ChangeClearsFlag()
    {
        // Arrange: admin reset mails a start password and ends the running session.
        var user = await _context.CreateUserAsync(Password);
        var before = await LoginAsync(user.Email!, Password);
        await _context.AccountsAsync(a => a.IssueStartPasswordAsync(user.Id, "de", default));
        var startPassword = _context.Mail.Last(MailTemplate.OneTimeCode).Values["Code"];

        // Act
        var withStartPassword = await LoginAsync(user.Email!, startPassword);
        var changed = await _context.AccountsAsync(a => a.ChangePasswordAsync(user.Id, startPassword, NewPassword, default));

        // Assert
        Assert.Null(await _context.SessionsAsync(s => s.RefreshAsync(before.Session!.RefreshToken, default)));
        Assert.Equal(LoginStatus.InvalidCredentials, (await LoginAsync(user.Email!, Password)).Status);
        Assert.True(withStartPassword.Session!.MustChangePassword);
        Assert.True(changed.Succeeded);
        Assert.False(changed.Session!.MustChangePassword);
        Assert.False((await LoginAsync(user.Email!, NewPassword)).Session!.MustChangePassword);
        Assert.False((await _context.FindUserAsync(user.Id))!.MustChangePassword);
    }

    [Fact]
    public async Task IssueStartPasswordAsync_After24Hours_StartPasswordNoLongerWorks()
    {
        var user = await _context.CreateUserAsync(Password);
        await _context.AccountsAsync(a => a.IssueStartPasswordAsync(user.Id, "de", default));
        var startPassword = _context.Mail.Last(MailTemplate.OneTimeCode).Values["Code"];

        _context.Time.Advance(TimeSpan.FromHours(24).Add(TimeSpan.FromMinutes(1)));

        Assert.Equal(LoginStatus.InvalidCredentials, (await LoginAsync(user.Email!, startPassword)).Status);
    }

    [Fact]
    public async Task ChangePasswordAsync_WrongCurrentPassword_Fails()
    {
        var user = await _context.CreateUserAsync(Password);

        var changed = await _context.AccountsAsync(a => a.ChangePasswordAsync(user.Id, "wrong-password", NewPassword, default));

        Assert.False(changed.Succeeded);
        Assert.Null(changed.Session);
    }

    private Task<LoginResult> LoginAsync(string email, string password) =>
        _context.SessionsAsync(s => s.LoginAsync(email, password, default));
}
