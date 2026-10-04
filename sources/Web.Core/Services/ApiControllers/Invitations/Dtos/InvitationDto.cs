using System.ComponentModel.DataAnnotations;
using Shared.Models.Organizations;

namespace Web.Core.Services.ApiControllers.Invitations.Dtos;

/// <summary>state: "Pending" or "Expired".</summary>
public sealed record InvitationDto(long Id, string Email, string Role, DateTimeOffset ExpiresAt, string State)
{
    public static InvitationDto From(InvitationInfo info) =>
        new(info.Id, info.Email, info.Role.ToString(), info.ExpiresAt, info.State.ToString());
}
