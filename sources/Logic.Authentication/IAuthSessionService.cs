using Logic.Authentication.Sessions;

namespace Logic.Authentication;

/// <summary>Login, refresh-token rotation and logout (ADR 013). Transport (cookies) is the caller's job.</summary>
public interface IAuthSessionService
{
    /// <summary>Checks the credentials (with lockout) and starts a session.</summary>
    Task<LoginResult> LoginAsync(string email, string password, CancellationToken cancellationToken);

    /// <summary>
    /// Redeems the refresh token (it can be used only once) and issues new tokens; <c>null</c> if it is unknown, expired
    /// or revoked. Redeeming an already used token revokes the whole chain – unless it was rotated only seconds ago
    /// (parallel refresh from several tabs, <see cref="JwtOptions.RefreshTokenReuseSeconds"/>).
    /// </summary>
    Task<AuthSession?> RefreshAsync(string refreshToken, CancellationToken cancellationToken);

    /// <summary>Ends the session: revokes every token of the refresh token's chain. Unknown tokens are ignored.</summary>
    Task LogoutAsync(string refreshToken, CancellationToken cancellationToken);
}
