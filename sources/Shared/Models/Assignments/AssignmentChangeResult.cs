using Shared.Enums;

namespace Shared.Models.Assignments;

/// <summary>On success the assignments created or changed by the call.</summary>
public sealed record AssignmentChangeResult(
    AssignmentChangeStatus Status,
    IReadOnlyList<AssignmentInfo>? Assignments = null,
    IReadOnlyDictionary<string, string[]>? Errors = null);
