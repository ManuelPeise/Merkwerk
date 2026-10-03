using System.ComponentModel.DataAnnotations;

namespace Web.Core.Services.ApiControllers.Setup.Dtos;

public sealed record SetupInitializeRequestDto(
    [Required, MaxLength(100)] string FamilyName,
    [Required, MaxLength(100)] string DisplayName,
    [Required, EmailAddress, MaxLength(256)] string Email,
    [Required, MaxLength(256)] string Password,
    bool PrivacyAccepted);
