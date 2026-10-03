using System.ComponentModel.DataAnnotations;
using Logic.Organizations.Members;

namespace Web.Core.Services.ApiControllers.Members.Dtos;

public sealed record RemoveMemberRequestDto([Range(1, long.MaxValue)] long MembershipId);
