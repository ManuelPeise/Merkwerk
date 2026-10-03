using Testcontainers.MySql;

namespace Logic.Organizations.Tests.Infrastructure;

[CollectionDefinition(Name)]
public sealed class SharedDatabaseCollection : ICollectionFixture<SharedDatabaseFixture>
{
    public const string Name = "organizations-database";
}
