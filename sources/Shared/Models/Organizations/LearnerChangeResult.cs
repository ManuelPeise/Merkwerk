using Shared.Enums;

namespace Shared.Models.Organizations;

public sealed record LearnerChangeResult(
    LearnerChangeStatus Status,
    LearnerInfo? Learner = null,
    IReadOnlyDictionary<string, string[]>? Errors = null);
