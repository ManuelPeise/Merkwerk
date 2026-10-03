namespace Logic.Organizations.Learners;

public sealed record LearnerChangeResult(
    LearnerChangeStatus Status,
    LearnerInfo? Learner = null,
    IReadOnlyDictionary<string, string[]>? Errors = null);
