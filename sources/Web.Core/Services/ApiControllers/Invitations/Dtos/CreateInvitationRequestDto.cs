using System.ComponentModel.DataAnnotations;
using Logic.Organizations.Invitations;

namespace Web.Core.Services.ApiControllers.Invitations.Dtos;

/// <summary>role: "Member" or "OrgAdmin".</summary>
public sealed record CreateInvitationRequestDto(
    [Required, EmailAddress, MaxLength(256)] string Email,
    [Required, RegularExpression("^(Member|OrgAdmin)$")] string Role);
