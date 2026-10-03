using System.ComponentModel.DataAnnotations;
using Logic.Organizations.Invitations;

namespace Web.Core.Services.ApiControllers.Invitations.Dtos;

public sealed record InvitationDetailsDto(string FamilyName, string Email, string InvitedBy, DateTimeOffset ExpiresAt)
{
    public static InvitationDetailsDto From(InvitationDetails details) =>
        new(details.FamilyName, details.Email, details.InvitedBy, details.ExpiresAt);
}
