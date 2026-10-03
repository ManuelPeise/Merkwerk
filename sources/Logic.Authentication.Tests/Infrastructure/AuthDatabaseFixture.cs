using Testcontainers.MySql;

namespace Logic.Authentication.Tests.Infrastructure;

/// <summary>One MySQL 8.4 container with all migrations applied, shared by the authentication test classes.</summary>
public sealed class AuthDatabaseFixture : IAsyncLifetime
{
    private readonly MySqlContainer _container = new MySqlBuilder("mysql:8.4").Build();

    public string ConnectionString => _container.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        var context = AuthTestContext.Create(this);
        await context.MigrateAsync();
        await context.Services.DisposeAsync();
    }

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();
}

[CollectionDefinition(Name)]
public sealed class AuthDatabaseCollection : ICollectionFixture<AuthDatabaseFixture>
{
    public const string Name = "auth-database";
}
