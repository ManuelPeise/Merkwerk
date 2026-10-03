using System.ComponentModel.DataAnnotations;

namespace Web.Core.Services.ApiControllers.Authentication.Dtos;

/// <summary>Token from the reset link (URL-safe encoded).</summary>
public sealed record ResetPasswordRequestDto(
    [Required, EmailAddress, MaxLength(256)] string Email,
    [Required, MaxLength(2048)] string Token,
    [Required, MaxLength(256)] string NewPassword);
