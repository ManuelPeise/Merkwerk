using Logic.Authentication.Sessions;
using Logic.Authentication.Tests.Infrastructure;

namespace Logic.Authentication.Tests;

/// <summary>LP-104: login with Identity, lockout, refresh-token rotation in MySQL (Testcontainers, needs Docker).</summary>
[Collection(AuthDatabaseCollection.Name)]
public sealed class AuthSessionServiceTests(AuthDatabaseFixture database)
{
    private const string Password = "correct-horse-battery";

    private readonly AuthTestContext _context = AuthTestContext.Create(database);

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsSessionWithBothTokens()
    {
        var user = await _context.CreateUserAsync(Password);

        var result = await _context.SessionsAsync(s => s.LoginAsync(user.Email!.ToUpperInvariant(), Password, default));

        Assert.Equal(LoginStatus.Success, result.Status);
        Assert.NotNull(result.Session);
        Assert.Equal("Anna", result.Session.Name);
        Assert.Equal(AuthRoles.Member, result.Session.Role);
        Assert.False(result.Session.MustChangePassword);
        Assert.Equal(_context.Time.GetUtcNow().AddMinutes(15), result.Session.AccessTokenExpiresAt);
        Assert.Equal(_context.Time.GetUtcNow().AddDays(14), result.Session.RefreshTokenExpiresAt);
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ReturnsInvalidCredentials()
    {
        var user = await _context.CreateUserAsync(Password);

        var result = await _context.SessionsAsync(s => s.LoginAsync(user.Email!, "wrong-password", default));

        Assert.Equal(LoginStatus.InvalidCredentials, result.Status);
        Assert.Null(result.Session);
    }

    [Fact]
    public async Task LoginAsync_UnknownEmail_ReturnsInvalidCredentials()
    {
        var result = await _context.SessionsAsync(s => s.LoginAsync("nobody@example.org", Password, default));

        Assert.Equal(LoginStatus.InvalidCredentials, result.Status);
    }

    [Fact]
    public async Task LoginAsync_FiveWrongPasswords_LocksOutEvenWithCorrectPassword()
    {
        var user = await _context.CreateUserAsync(Password);

        for (var attempt = 1; attempt <= 5; attempt++)
        {
            await _context.SessionsAsync(s => s.LoginAsync(user.Email!, "wrong-password", default));
        }

        var withCorrectPassword = await _context.SessionsAsync(s => s.LoginAsync(user.Email!, Password, default));

        Assert.Equal(LoginStatus.InvalidCredentials, withCorrectPassword.Status);
        Assert.Null(withCorrectPassword.Session);
    }

    [Fact]
    public async Task LoginAsync_LockedAccount_AnswersLikeUnknownEmail()
    {
        var user = await _context.CreateUserAsync(Password);
        var answers = new List<LoginStatus>();
        var unknownAnswers = new List<LoginStatus>();

        for (var attempt = 1; attempt <= 6; attempt++)
        {
            answers.Add((await _context.SessionsAsync(s => s.LoginAsync(user.Email!, "wrong-password", default))).Status);
            unknownAnswers.Add((await _context.SessionsAsync(s => s.LoginAsync("nobody@example.org", "wrong-password", default))).Status);
        }

        Assert.Equal(unknownAnswers, answers);
    }


    [Fact]
    public async Task LoginAsync_EmailNotConfirmed_ReturnsEmailNotConfirmed()
    {
        var user = await _context.CreateUserAsync(Password, emailConfirmed: false);

        var result = await _context.SessionsAsync(s => s.LoginAsync(user.Email!, Password, default));

        Assert.Equal(LoginStatus.EmailNotConfirmed, result.Status);
        Assert.Null(result.Session);
    }

    [Fact]
    public async Task RefreshAsync_ValidToken_ReturnsNewSessionWithNewRefreshToken()
    {
        var login = await LoginAsync();

        var refreshed = await _context.SessionsAsync(s => s.RefreshAsync(login.RefreshToken, default));

        Assert.NotNull(refreshed);
        Assert.NotEqual(login.RefreshToken, refreshed.RefreshToken);
    }

    [Fact]
    public async Task RefreshAsync_UsedTokenPresentedAgain_RevokesWholeChain()
    {
        // Arrange: rotate once, so the first token is "used".
        var login = await LoginAsync();
        var second = await _context.SessionsAsync(s => s.RefreshAsync(login.RefreshToken, default));

        // Act: someone replays the first token after the reuse window.
        _context.Time.Advance(TimeSpan.FromSeconds(31));
        var replay = await _context.SessionsAsync(s => s.RefreshAsync(login.RefreshToken, default));
        var legitimate = await _context.SessionsAsync(s => s.RefreshAsync(second!.RefreshToken, default));

        // Assert: the replay fails and takes the current token of the chain with it.
        Assert.Null(replay);
        Assert.Null(legitimate);
    }

    [Fact]
    public async Task RefreshAsync_RotatedTokenWithinReuseWindow_ReturnsSessionAndKeepsChain()
    {
        // Arrange: tab A rotates the shared cookie.
        var login = await LoginAsync();
        var tabA = await _context.SessionsAsync(s => s.RefreshAsync(login.RefreshToken, default));

        // Act: tab B still sends the old cookie a few seconds later.
        _context.Time.Advance(TimeSpan.FromSeconds(5));
        var tabB = await _context.SessionsAsync(s => s.RefreshAsync(login.RefreshToken, default));

        // Assert: both tabs stay signed in.
        Assert.NotNull(tabB);
        Assert.NotNull(await _context.SessionsAsync(s => s.RefreshAsync(tabA!.RefreshToken, default)));
        Assert.NotNull(await _context.SessionsAsync(s => s.RefreshAsync(tabB.RefreshToken, default)));
    }

    [Fact]
    public async Task RefreshAsync_ParallelRequestsWithSameToken_AllSucceed()
    {
        var login = await LoginAsync();

        var results = await Task.WhenAll(Enumerable.Range(0, 4)
            .Select(_ => _context.SessionsAsync(s => s.RefreshAsync(login.RefreshToken, default))));

        Assert.All(results, Assert.NotNull);
        Assert.Equal(results.Length, results.Select(r => r!.RefreshToken).Distinct().Count());
    }

    [Fact]
    public async Task RefreshAsync_RotatedTokenAfterLogout_ReturnsNull()
    {
        // Arrange: rotate, then log out with the current token.
        var login = await LoginAsync();
        var current = await _context.SessionsAsync(s => s.RefreshAsync(login.RefreshToken, default));
        await _context.SessionsAsync(async s =>
        {
            await s.LogoutAsync(current!.RefreshToken, default);
            return true;
        });

        // Act: the previous token is presented within the reuse window.
        var replay = await _context.SessionsAsync(s => s.RefreshAsync(login.RefreshToken, default));

        // Assert: the session is over, the reuse window does not revive it.
        Assert.Null(replay);
    }

    [Fact]
    public async Task RefreshAsync_UserLockedOutMeanwhile_ReturnsNullAndEndsSession()
    {
        // Arrange
        var user = await _context.CreateUserAsync(Password);
        var login = (await _context.SessionsAsync(s => s.LoginAsync(user.Email!, Password, default))).Session!;
        for (var attempt = 1; attempt <= 5; attempt++)
        {
            await _context.SessionsAsync(s => s.LoginAsync(user.Email!, "wrong-password", default));
        }

        // Act
        var refreshed = await _context.SessionsAsync(s => s.RefreshAsync(login.RefreshToken, default));

        // Assert
        Assert.Null(refreshed);
    }

    [Fact]
    public async Task RefreshAsync_ExpiredToken_ReturnsNull()
    {
        var login = await LoginAsync();
        _context.Time.Advance(TimeSpan.FromDays(14).Add(TimeSpan.FromSeconds(1)));

        Assert.Null(await _context.SessionsAsync(s => s.RefreshAsync(login.RefreshToken, default)));
    }

    [Fact]
    public async Task RefreshAsync_UnknownToken_ReturnsNull()
    {
        Assert.Null(await _context.SessionsAsync(s => s.RefreshAsync(TokenService.CreateRefreshToken(), default)));
    }

    [Fact]
    public async Task LogoutAsync_ThenRefresh_ReturnsNull()
    {
        var login = await LoginAsync();

        await _context.SessionsAsync(async s =>
        {
            await s.LogoutAsync(login.RefreshToken, default);
            return true;
        });

        Assert.Null(await _context.SessionsAsync(s => s.RefreshAsync(login.RefreshToken, default)));
    }

    private async Task<AuthSession> LoginAsync()
    {
        var user = await _context.CreateUserAsync(Password);
        var result = await _context.SessionsAsync(s => s.LoginAsync(user.Email!, Password, default));
        return result.Session ?? throw new InvalidOperationException($"Login failed: {result.Status}");
    }
}
