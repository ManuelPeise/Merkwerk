using Shared.Models.Organizations;

namespace Shared.Enums;

public enum SetupStatus
{
    Success,

    /// <summary>The instance is already set up (409).</summary>
    AlreadyDone,

    /// <summary>Input or password rules not met (400, see <c>SetupResult.Errors</c>).</summary>
    Invalid,
}
