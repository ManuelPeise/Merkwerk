using System.ComponentModel.DataAnnotations;

namespace Web.Core.Services.ApiControllers.Authentication.Dtos;

/// <summary>E-mail and token from the reset link (URL-safe encoded), checked when the page opens (LP-166).</summary>
public sealed record VerifyResetTokenRequestDto(
    [Required, EmailAddress, MaxLength(256)] string Email,
    [Required, MaxLength(2048)] string Token);
