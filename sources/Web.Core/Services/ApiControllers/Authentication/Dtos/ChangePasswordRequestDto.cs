using System.ComponentModel.DataAnnotations;

namespace Web.Core.Services.ApiControllers.Authentication.Dtos;

public sealed record ChangePasswordRequestDto(
    [Required, MaxLength(256)] string CurrentPassword,
    [Required, MaxLength(256)] string NewPassword);
