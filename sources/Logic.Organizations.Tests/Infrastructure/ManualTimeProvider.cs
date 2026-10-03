using Data.Database.Abstractions;
using Logic.Notifications;

namespace Logic.Organizations.Tests.Infrastructure;

/// <summary>Time that only moves when the test says so.</summary>
public sealed class ManualTimeProvider(DateTimeOffset start) : TimeProvider
{
    private DateTimeOffset _now = start;

    public override DateTimeOffset GetUtcNow() => _now;

    public void Advance(TimeSpan by) => _now = _now.Add(by);
}
