namespace Logic.Authentication.Tokens;

/// <summary>A refresh token in plain text (only ever handed to the client) with its user and chain.</summary>
internal sealed record IssuedRefreshToken(long UserId, Guid ChainId, string Token, DateTimeOffset ExpiresAt);
