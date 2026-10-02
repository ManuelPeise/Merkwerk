namespace Shared.Api.Auth;

/// <summary>Credentials of an adult. LP-006 spike: checked against a demo user from configuration.</summary>
public sealed record LoginRequest(string Email, string Password);
