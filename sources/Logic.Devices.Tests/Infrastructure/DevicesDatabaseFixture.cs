using Testcontainers.MySql;

namespace Logic.Devices.Tests.Infrastructure;

/// <summary>MySQL 8.4 container with all migrations applied, shared by all device tests (each test creates its own families).</summary>
public sealed class DevicesDatabaseFixture : IAsyncLifetime
{
    private readonly MySqlContainer _container = new MySqlBuilder("mysql:8.4").Build();

    public string ConnectionString => _container.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        var context = DevicesTestContext.Create(ConnectionString);
        await context.MigrateAsync();
        await context.Services.DisposeAsync();
    }

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();
}
