using System.ComponentModel.DataAnnotations;

namespace Web.Core.Services.ApiControllers.Authentication.Dtos;

/// <summary>Values from the confirmation link; the user id comes as string from the query string.</summary>
public sealed record ConfirmEmailRequestDto(
    [Required, MaxLength(20)] string UserId,
    [Required, MaxLength(2048)] string Token);
