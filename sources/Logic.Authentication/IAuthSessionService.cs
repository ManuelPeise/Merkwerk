namespace Logic.Authentication;

/// <summary>Login, refresh-token rotation and logout (ADR 013). Transport (cookies) is the caller's job.</summary>
public interface IAuthSessionService
{
    /// <summary>Checks the credentials and starts a session; <c>null</c> if they are wrong.</summary>
    Task<AuthSession?> LoginAsync(string email, string password, CancellationToken cancellationToken);

    /// <summary>Redeems the refresh token (it can be used only once) and issues new tokens; <c>null</c> if it is invalid or expired.</summary>
    Task<AuthSession?> RefreshAsync(string refreshToken, CancellationToken cancellationToken);

    /// <summary>Revokes the refresh token. Unknown tokens are ignored.</summary>
    Task LogoutAsync(string refreshToken, CancellationToken cancellationToken);
}
