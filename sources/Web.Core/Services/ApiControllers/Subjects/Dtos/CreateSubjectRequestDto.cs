using System.ComponentModel.DataAnnotations;
using Shared.Models.Subjects;

namespace Web.Core.Services.ApiControllers.Subjects.Dtos;

/// <summary>Color, icon and language must come from the fixed choice – the service checks that (field errors).</summary>
public sealed record CreateSubjectRequestDto(
    [Required, MaxLength(50)] string Name,
    [Required, MaxLength(10)] string LanguageCode,
    [Required, MaxLength(50)] string Color,
    [Required, MaxLength(50)] string Icon)
{
    public SubjectInput ToInput() => new(Name, LanguageCode, Color, Icon);
}
