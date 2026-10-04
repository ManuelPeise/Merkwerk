using Testcontainers.MySql;

namespace Logic.Content.Tests.Infrastructure;

/// <summary>MySQL 8.4 container with all migrations applied (seed: Deutsch, Englisch, Mathe).</summary>
public sealed class ContentDatabaseFixture : IAsyncLifetime
{
    private readonly MySqlContainer _container = new MySqlBuilder("mysql:8.4").Build();

    public string ConnectionString => _container.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        var context = ContentTestContext.Create(ConnectionString);
        await context.MigrateAsync();
        await context.Services.DisposeAsync();
    }

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();
}
