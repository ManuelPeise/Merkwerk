using System.ComponentModel.DataAnnotations;

namespace Web.Core.Services.ApiControllers.Learners.Dtos;

public sealed record UpdateLearnerRequestDto(
    [Range(1, long.MaxValue)] long Id,
    [Required, MaxLength(30)] string DisplayName,
    [Range(1, 4)] int Grade,
    [Required, MaxLength(30)] string AvatarId);
