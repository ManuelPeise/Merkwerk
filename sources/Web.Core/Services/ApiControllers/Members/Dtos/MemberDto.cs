using System.ComponentModel.DataAnnotations;
using Shared.Models.Organizations;

namespace Web.Core.Services.ApiControllers.Members.Dtos;

public sealed record MemberDto(long MembershipId, long UserId, string DisplayName, string Email, string Role, bool IsOwner)
{
    public static MemberDto From(MemberInfo info) =>
        new(info.MembershipId, info.UserId, info.DisplayName, info.Email, info.Role.ToString(), info.IsOwner);
}
