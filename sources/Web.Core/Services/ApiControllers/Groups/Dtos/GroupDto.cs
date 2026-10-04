using Shared.Models.Organizations;

namespace Web.Core.Services.ApiControllers.Groups.Dtos;

public sealed record GroupDto(long Id, string Name, IReadOnlyList<long> LearnerIds)
{
    public static GroupDto From(GroupInfo info) => new(info.Id, info.Name, info.LearnerIds);
}
