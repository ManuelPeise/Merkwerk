using System.ComponentModel.DataAnnotations;

namespace Web.Core.Services.ApiControllers.Authentication.Dtos;

public sealed record ForgotPasswordRequestDto([Required, EmailAddress, MaxLength(256)] string Email);
