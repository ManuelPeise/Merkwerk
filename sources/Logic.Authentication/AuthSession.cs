namespace Logic.Authentication;

/// <summary>Result of a successful login or refresh: the tokens plus what the client may know about the user.</summary>
public sealed record AuthSession(
    string Name,
    string Role,
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAt,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAt);
