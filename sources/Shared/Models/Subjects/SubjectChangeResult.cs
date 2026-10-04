using Shared.Enums;

namespace Shared.Models.Subjects;

public sealed record SubjectChangeResult(
    SubjectChangeStatus Status,
    SubjectInfo? Subject = null,
    IReadOnlyDictionary<string, string[]>? Errors = null);
