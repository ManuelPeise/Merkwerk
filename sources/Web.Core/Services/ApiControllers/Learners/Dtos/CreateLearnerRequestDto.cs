using System.ComponentModel.DataAnnotations;
using Logic.Organizations.Learners;

namespace Web.Core.Services.ApiControllers.Learners.Dtos;

public sealed record CreateLearnerRequestDto(
    [Required, MaxLength(30)] string DisplayName,
    [Range(1, 4)] int Grade,
    [Required, MaxLength(30)] string AvatarId);
