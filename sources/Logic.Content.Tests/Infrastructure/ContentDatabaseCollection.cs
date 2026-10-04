namespace Logic.Content.Tests.Infrastructure;

[CollectionDefinition(Name)]
public sealed class ContentDatabaseCollection : ICollectionFixture<ContentDatabaseFixture>
{
    public const string Name = "content-database";
}
