using System.Text.Json.Serialization;
using Shared.Models.Authentication;

namespace Web.Core.Services.ApiControllers.Authentication.Dtos;

/// <summary>
/// What the client may know about a session. The tokens themselves stay in HttpOnly cookies.
/// <c>avatarId</c> only for children (LP-106).
/// </summary>
public sealed record SessionDto(
    string Name,
    string Role,
    bool MustChangePassword,
    DateTimeOffset AccessTokenExpiresAt,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? AvatarId = null)
{
    public static SessionDto From(AuthSession session) =>
        new(session.Name, session.Role, session.MustChangePassword, session.AccessTokenExpiresAt, session.AvatarId);
}
