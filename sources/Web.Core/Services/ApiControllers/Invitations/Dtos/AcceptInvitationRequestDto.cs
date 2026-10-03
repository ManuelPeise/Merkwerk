using System.ComponentModel.DataAnnotations;
using Logic.Organizations.Invitations;

namespace Web.Core.Services.ApiControllers.Invitations.Dtos;

/// <summary>New account: displayName, password, privacyAccepted. Signed in: only the token.</summary>
public sealed record AcceptInvitationRequestDto(
    [Required, MaxLength(100)] string Token,
    [MaxLength(100)] string? DisplayName,
    [MaxLength(256)] string? Password,
    bool PrivacyAccepted);
