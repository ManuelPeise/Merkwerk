using Testcontainers.MySql;

namespace Data.IntegrationTests.Infrastructure;

/// <summary>
/// One throwaway MySQL 8.4 container per test class (needs Docker). The schema is created once with
/// EnsureCreated – migrations come with LP-103.
/// </summary>
public sealed class MySqlFixture : IAsyncLifetime
{
    private readonly MySqlContainer _container = new MySqlBuilder()
        .WithImage("mysql:8.4")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        await using var context = TestDbContext.Create(ConnectionString, new TestCurrentUser());
        await context.Database.EnsureCreatedAsync();
    }

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();
}
