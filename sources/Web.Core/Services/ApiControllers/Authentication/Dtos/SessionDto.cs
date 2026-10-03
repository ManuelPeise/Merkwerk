using Logic.Authentication;

namespace Web.Core.Services.ApiControllers.Authentication.Dtos;

/// <summary>What the client may know about a session. The tokens themselves stay in HttpOnly cookies.</summary>
public sealed record SessionDto(string Name, string Role, DateTimeOffset AccessTokenExpiresAt)
{
    public static SessionDto From(AuthSession session) =>
        new(session.Name, session.Role, session.AccessTokenExpiresAt);
}
