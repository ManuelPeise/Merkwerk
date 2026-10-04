namespace Shared.Models.Organizations;

/// <summary>A group with the ids of its children (sorted).</summary>
public sealed record GroupInfo(long Id, string Name, IReadOnlyList<long> LearnerIds);
