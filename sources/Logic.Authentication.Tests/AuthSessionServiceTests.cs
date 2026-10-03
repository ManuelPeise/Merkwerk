using Microsoft.Extensions.Options;

namespace Logic.Authentication.Tests;

/// <summary>LP-006 spike service: demo login, refresh-token rotation and logout (ADR 013).</summary>
public sealed class AuthSessionServiceTests
{
    private const string DemoUser = "demo@merkwerk.local";
    private const string DemoPassword = "demo-password";

    private readonly ManualTimeProvider _time = new(TestSettings.Start);

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsSessionWithBothTokens()
    {
        var service = CreateService();

        var session = await service.LoginAsync("DEMO@merkwerk.local", DemoPassword, CancellationToken.None);

        Assert.NotNull(session);
        Assert.Equal(DemoUser, session.Name);
        Assert.False(string.IsNullOrEmpty(session.AccessToken));
        Assert.False(string.IsNullOrEmpty(session.RefreshToken));
        Assert.Equal(TestSettings.Start.AddMinutes(15), session.AccessTokenExpiresAt);
        Assert.Equal(TestSettings.Start.AddDays(14), session.RefreshTokenExpiresAt);
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ReturnsNull()
    {
        var service = CreateService();

        Assert.Null(await service.LoginAsync(DemoUser, "wrong", CancellationToken.None));
    }

    [Fact]
    public async Task LoginAsync_NoDemoUserConfigured_ReturnsNull()
    {
        var service = CreateService(new DemoUserOptions());

        Assert.Null(await service.LoginAsync(string.Empty, string.Empty, CancellationToken.None));
    }

    [Fact]
    public async Task RefreshAsync_ValidToken_ReturnsNewSessionWithNewRefreshToken()
    {
        var service = CreateService();
        var login = await LoginAsync(service);

        var refreshed = await service.RefreshAsync(login.RefreshToken, CancellationToken.None);

        Assert.NotNull(refreshed);
        Assert.Equal(login.Name, refreshed.Name);
        Assert.NotEqual(login.RefreshToken, refreshed.RefreshToken);
    }

    [Fact]
    public async Task RefreshAsync_TokenUsedTwice_SecondUseReturnsNull()
    {
        // Rotation: every refresh token can be redeemed exactly once.
        var service = CreateService();
        var login = await LoginAsync(service);
        await service.RefreshAsync(login.RefreshToken, CancellationToken.None);

        var second = await service.RefreshAsync(login.RefreshToken, CancellationToken.None);

        Assert.Null(second);
    }

    [Fact]
    public async Task RefreshAsync_ExpiredToken_ReturnsNull()
    {
        var service = CreateService();
        var login = await LoginAsync(service);
        _time.Advance(TimeSpan.FromDays(14).Add(TimeSpan.FromSeconds(1)));

        Assert.Null(await service.RefreshAsync(login.RefreshToken, CancellationToken.None));
    }

    [Fact]
    public async Task RefreshAsync_UnknownToken_ReturnsNull()
    {
        var service = CreateService();

        Assert.Null(await service.RefreshAsync(TokenService.CreateRefreshToken(), CancellationToken.None));
    }

    [Fact]
    public async Task LogoutAsync_ThenRefresh_ReturnsNull()
    {
        var service = CreateService();
        var login = await LoginAsync(service);

        await service.LogoutAsync(login.RefreshToken, CancellationToken.None);

        Assert.Null(await service.RefreshAsync(login.RefreshToken, CancellationToken.None));
    }

    private AuthSessionService CreateService(DemoUserOptions? demoUser = null)
    {
        var jwt = TestSettings.Jwt();

        return new AuthSessionService(
            new TokenService(jwt, _time),
            new InMemoryRefreshTokenStore(jwt, _time),
            Options.Create(demoUser ?? new DemoUserOptions { DemoUser = DemoUser, DemoPassword = DemoPassword }));
    }

    private static async Task<AuthSession> LoginAsync(AuthSessionService service) =>
        await service.LoginAsync(DemoUser, DemoPassword, CancellationToken.None)
        ?? throw new InvalidOperationException("Demo login failed.");
}
