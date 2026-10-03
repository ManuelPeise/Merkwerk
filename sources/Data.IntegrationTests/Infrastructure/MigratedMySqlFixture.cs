using Data.Database;
using Microsoft.EntityFrameworkCore;
using Testcontainers.MySql;

namespace Data.IntegrationTests.Infrastructure;

/// <summary>
/// Empty MySQL 8.4 container on which all migrations are applied once (LP-103). Separate from
/// <see cref="MySqlFixture"/>, because a schema made by EnsureCreated cannot be migrated.
/// </summary>
public sealed class MigratedMySqlFixture : IAsyncLifetime
{
    private readonly MySqlContainer _container = new MySqlBuilder(MySqlImage.Name).Build();

    public string ConnectionString => _container.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        await using var context = CreateContext();
        await context.Database.MigrateAsync();
    }

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();

    public MerkwerkDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<MerkwerkDbContext>()
            .UseMySQL(ConnectionString)
            .Options;

        return new MerkwerkDbContext(options, new TestCurrentUser());
    }
}
