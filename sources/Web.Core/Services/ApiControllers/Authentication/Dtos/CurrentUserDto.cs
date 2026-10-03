using System.Text.Json.Serialization;

namespace Web.Core.Services.ApiControllers.Authentication.Dtos;

/// <summary>The signed-in user or child, read from the access token. <c>avatarId</c> only for children (LP-106).</summary>
public sealed record CurrentUserDto(
    string Name,
    string Role,
    bool MustChangePassword,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? AvatarId = null);
