using System.ComponentModel.DataAnnotations;
using Shared.Enums;

namespace Web.Core.Services.ApiControllers.Invitations.Dtos;

/// <summary>role: "Member" or "OrgAdmin".</summary>
public sealed record CreateInvitationRequestDto(
    [Required, EmailAddress, MaxLength(256)] string Email,
    [Required, RegularExpression("^(Member|OrgAdmin)$")] string Role)
{
    /// <summary>The validated <see cref="Role"/> as enum (the regular expression allows only these two values).</summary>
    public OrganizationRole ToRole() => Role == nameof(OrganizationRole.OrgAdmin)
        ? OrganizationRole.OrgAdmin
        : OrganizationRole.Member;
}
