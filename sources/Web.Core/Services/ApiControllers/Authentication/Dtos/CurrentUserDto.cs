namespace Web.Core.Services.ApiControllers.Authentication.Dtos;

/// <summary>The signed-in user, read from the access token.</summary>
public sealed record CurrentUserDto(string Name, string Role);
