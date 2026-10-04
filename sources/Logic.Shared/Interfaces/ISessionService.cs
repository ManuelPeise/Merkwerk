using Shared.Models.Authentication;

namespace Logic.Shared.Interfaces;

/// <summary>
/// One refresh and logout path for every kind of session: adults (refresh-token chain, ADR 013) and children on
/// paired devices (LP-106). The client knows only one refresh cookie; this service finds out which kind it is.
/// Transport (cookies) is the caller's job.
/// </summary>
public interface ISessionService
{
    /// <summary>
    /// New tokens for the refresh token – tried as an adult token first, then as a child's session; <c>null</c> if
    /// neither knows it (unknown, expired, revoked or the device was unpaired).
    /// </summary>
    Task<AuthSession?> RefreshAsync(string refreshToken, CancellationToken cancellationToken);

    /// <summary>Ends the session of the refresh token, whichever kind it is. Unknown tokens are ignored.</summary>
    Task LogoutAsync(string refreshToken, CancellationToken cancellationToken);
}
