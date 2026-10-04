using Shared.Models.Subjects;

namespace Web.Core.Services.ApiControllers.Subjects.Dtos;

/// <summary><see cref="Color"/>: design-token key (e.g. "subject.math"); <see cref="Icon"/>: icon key (e.g. "math").</summary>
public sealed record SubjectDto(long Id, string Name, string LanguageCode, string Color, string Icon)
{
    public static SubjectDto From(SubjectInfo info) => new(info.Id, info.Name, info.LanguageCode, info.Color, info.Icon);
}
