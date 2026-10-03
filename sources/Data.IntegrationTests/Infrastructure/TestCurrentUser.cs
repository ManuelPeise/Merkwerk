using Data.Database.Abstractions;

namespace Data.IntegrationTests.Infrastructure;

public sealed class TestCurrentUser(string actor = SystemCurrentUser.SystemActor, long? organizationId = null) : ICurrentUser
{
    public string Actor { get; } = actor;

    public long? OrganizationId { get; } = organizationId;
}

/// <summary>Time that only moves when the test says so.</summary>
public sealed class ManualTimeProvider(DateTimeOffset start) : TimeProvider
{
    private DateTimeOffset _now = start;

    public override DateTimeOffset GetUtcNow() => _now;

    public void Advance(TimeSpan by) => _now = _now.Add(by);
}
