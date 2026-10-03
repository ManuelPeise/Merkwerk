using Testcontainers.MySql;

namespace Logic.Organizations.Tests.Infrastructure;

/// <summary>MySQL 8.4 container with all migrations applied.</summary>
public abstract class DatabaseFixture : IAsyncLifetime
{
    private readonly MySqlContainer _container = new MySqlBuilder("mysql:8.4").Build();

    public string ConnectionString => _container.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        var context = OrganizationsTestContext.Create(ConnectionString);
        await context.MigrateAsync();
        await context.Services.DisposeAsync();
    }

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();
}
