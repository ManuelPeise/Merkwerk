namespace Logic.Content.Assignments;

/// <summary>Limits for assigning exercises (LP-114).</summary>
public static class AssignmentRules
{
    /// <summary>Children plus groups in one call (a family has at most 10 children and 20 groups).</summary>
    public const int MaxTargets = 30;

    /// <summary>
    /// How many days a due date may lie before today (UTC). One day, so "today" in a time zone behind UTC is never
    /// rejected.
    /// </summary>
    public const int DueDateToleranceDays = 1;
}
