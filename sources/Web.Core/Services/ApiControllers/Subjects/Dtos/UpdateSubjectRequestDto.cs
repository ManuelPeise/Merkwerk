using System.ComponentModel.DataAnnotations;
using Shared.Models.Subjects;

namespace Web.Core.Services.ApiControllers.Subjects.Dtos;

public sealed record UpdateSubjectRequestDto(
    [Range(1, long.MaxValue)] long Id,
    [Required, MaxLength(50)] string Name,
    [Required, MaxLength(10)] string LanguageCode,
    [Required, MaxLength(50)] string Color,
    [Required, MaxLength(50)] string Icon)
{
    public SubjectInput ToInput() => new(Name, LanguageCode, Color, Icon);
}
