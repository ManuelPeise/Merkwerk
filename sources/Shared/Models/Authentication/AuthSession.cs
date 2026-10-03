namespace Shared.Models.Authentication;

/// <summary>
/// Result of a successful login or refresh: the tokens plus what the client may know about the user.
/// <see cref="AvatarId"/> is only set for children (LP-106).
/// </summary>
public sealed record AuthSession(
    string Name,
    string Role,
    bool MustChangePassword,
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAt,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAt,
    string? AvatarId = null);
