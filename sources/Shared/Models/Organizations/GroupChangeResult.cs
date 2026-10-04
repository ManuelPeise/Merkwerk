using Shared.Enums;

namespace Shared.Models.Organizations;

public sealed record GroupChangeResult(
    GroupChangeStatus Status,
    GroupInfo? Group = null,
    IReadOnlyDictionary<string, string[]>? Errors = null);
