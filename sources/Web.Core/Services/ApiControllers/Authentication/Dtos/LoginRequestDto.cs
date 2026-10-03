using System.ComponentModel.DataAnnotations;

namespace Web.Core.Services.ApiControllers.Authentication.Dtos;

/// <summary>Credentials for the login. Validated automatically ([ApiController] → 400 ValidationProblemDetails).</summary>
public sealed record LoginRequestDto(
    [Required, EmailAddress, MaxLength(256)] string Email,
    [Required, MaxLength(256)] string Password);
