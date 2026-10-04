namespace Shared.Models.Organizations;

/// <summary>Name and exactly the children the group should hold.</summary>
public sealed record GroupInput(string Name, IReadOnlyList<long> LearnerIds);
