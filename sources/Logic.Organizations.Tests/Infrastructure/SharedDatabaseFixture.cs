using Testcontainers.MySql;

namespace Logic.Organizations.Tests.Infrastructure;

/// <summary>Shared by the invitation, member and learner tests (every test creates its own families).</summary>
public sealed class SharedDatabaseFixture : DatabaseFixture
{
}
