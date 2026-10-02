namespace Shared.Api.Auth;

/// <summary>Information about the current session returned by the auth endpoints.</summary>
public sealed record SessionInfo(string Name, string Role, DateTimeOffset AccessTokenExpiresAt);
