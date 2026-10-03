using Testcontainers.MySql;

namespace Logic.Organizations.Tests.Infrastructure;

/// <summary>Own, empty database: the first-run setup works exactly once per instance.</summary>
public sealed class SetupDatabaseFixture : DatabaseFixture
{
}
