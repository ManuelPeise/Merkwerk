namespace Logic.Organizations.Setup;

public enum SetupStatus
{
    Success,

    /// <summary>The instance is already set up (409).</summary>
    AlreadyDone,

    /// <summary>Input or password rules not met (400, see <see cref="SetupResult.Errors"/>).</summary>
    Invalid,
}
