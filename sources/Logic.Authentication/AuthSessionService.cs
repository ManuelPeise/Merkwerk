using Data.Database.Entities.Identity;
using Logic.Authentication.Sessions;
using Logic.Authentication.Tokens;
using Microsoft.AspNetCore.Identity;

namespace Logic.Authentication;

/// <summary>Login against ASP.NET Core Identity, JWT access tokens and refresh tokens in the database (LP-104).</summary>
internal sealed class AuthSessionService(
    UserManager<User> userManager,
    TokenService tokenService,
    RefreshTokenStore refreshTokens,
    TimeProvider timeProvider) : IAuthSessionService
{
    public async Task<LoginResult> LoginAsync(string email, string password, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(email);

        if (user is null)
        {
            return new LoginResult(LoginStatus.InvalidCredentials);
        }

        // A locked account answers like a wrong password: a distinct answer would tell an attacker which addresses
        // are registered (unknown addresses never lock).
        if (await userManager.IsLockedOutAsync(user))
        {
            return new LoginResult(LoginStatus.InvalidCredentials);
        }

        if (!await userManager.CheckPasswordAsync(user, password))
        {
            await userManager.AccessFailedAsync(user);
            return new LoginResult(LoginStatus.InvalidCredentials);
        }

        if (IsStartPasswordExpired(user))
        {
            return new LoginResult(LoginStatus.InvalidCredentials);
        }

        await userManager.ResetAccessFailedCountAsync(user);

        if (!user.EmailConfirmed)
        {
            return new LoginResult(LoginStatus.EmailNotConfirmed);
        }

        return new LoginResult(LoginStatus.Success, await IssueSessionAsync(user, Guid.NewGuid(), cancellationToken));
    }

    public async Task<AuthSession?> RefreshAsync(string refreshToken, CancellationToken cancellationToken)
    {
        var rotated = await refreshTokens.RotateAsync(refreshToken, cancellationToken);

        if (rotated is null)
        {
            return null;
        }

        var user = await userManager.FindByIdAsync(rotated.UserId.ToString(System.Globalization.CultureInfo.InvariantCulture));

        if (user is null || await userManager.IsLockedOutAsync(user))
        {
            await refreshTokens.RevokeChainAsync(rotated.ChainId, cancellationToken);
            return null;
        }

        return CreateSession(user, rotated);
    }

    public Task LogoutAsync(string refreshToken, CancellationToken cancellationToken) =>
        refreshTokens.RevokeChainAsync(refreshToken, cancellationToken);

    /// <summary>New access token plus refresh token in the given chain (new chain = new login).</summary>
    internal async Task<AuthSession> IssueSessionAsync(User user, Guid chainId, CancellationToken cancellationToken) =>
        CreateSession(user, await refreshTokens.IssueAsync(user.Id, chainId, cancellationToken));

    private AuthSession CreateSession(User user, IssuedRefreshToken refreshToken)
    {
        // Until memberships exist (LP-105/LP-107) every adult is a member.
        const string role = AuthRoles.Member;
        var name = string.IsNullOrEmpty(user.DisplayName) ? user.Email ?? string.Empty : user.DisplayName;

        var (accessToken, accessExpiresAt) = tokenService.CreateAccessToken(
            user.Id.ToString(System.Globalization.CultureInfo.InvariantCulture), name, role, user.MustChangePassword);

        return new AuthSession(
            name, role, user.MustChangePassword, accessToken, accessExpiresAt, refreshToken.Token, refreshToken.ExpiresAt);
    }

    private bool IsStartPasswordExpired(User user) =>
        user.MustChangePassword
        && user.StartPasswordExpiresAt is { } expiresAt
        && expiresAt <= timeProvider.GetUtcNow().UtcDateTime;
}
