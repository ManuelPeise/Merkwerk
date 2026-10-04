namespace Shared.Models.Subjects;

/// <summary>A subject as shown everywhere: <see cref="Color"/> is a design-token key, <see cref="Icon"/> an icon key.</summary>
public sealed record SubjectInfo(long Id, string Name, string LanguageCode, string Color, string Icon);
