using System.ComponentModel.DataAnnotations;
using Logic.Organizations.Learners;

namespace Web.Core.Services.ApiControllers.Learners.Dtos;

public sealed record LearnerDto(long Id, string DisplayName, int Grade, string AvatarId)
{
    public static LearnerDto From(LearnerInfo info) => new(info.Id, info.DisplayName, info.Grade, info.AvatarId);
}
